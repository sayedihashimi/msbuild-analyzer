namespace MsbuildAnalyzer.Common {
    using Microsoft.Build.Execution;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MsbuildAnalyzer.Common.Extensions;
    public class MSBuildComparer {

        public class ProjectInstanceCompareResult {
            public bool AreEqual { get; set; }
            public PropertyListCompareResult PropertyCompareResult { get; set; }
            public ItemListCollectionCompareResult ItemListColCompareResult { get; set; }
        }

        private ProjectItemInstanceComparer _piiComparer;
        public MSBuildComparer() {
            _piiComparer = new ProjectItemInstanceComparer();
        }
        public ProjectInstanceCompareResult Compare(ProjectInstance left, ProjectInstance right) {
            var result = new ProjectInstanceCompareResult {
                PropertyCompareResult = Compare(left.Properties, right.Properties),
                ItemListColCompareResult = Compare(left.Items, right.Items)
            };

            if(result.PropertyCompareResult.AreEqual &&
                result.ItemListColCompareResult.AreEqual) {
                    result.AreEqual = true;
            }
            else {
                result.AreEqual = false;
            }

            return result;
        }
        public PropertyListCompareResult Compare(ICollection<ProjectPropertyInstance> left, ICollection<ProjectPropertyInstance> right) {

            var leftArray = left.ToArray();
            var rightArray = right.ToArray();

            // do a quick compare to see if they are the same or not
            if (ArraysEqual(leftArray, rightArray)) {
                return new PropertyListCompareResult { AreEqual = true };
            }

            // create a couple indexes
            List<string> leftPropertyNames = new List<string>(left.Count);
            List<string> rightPropertyNames = new List<string>(right.Count);

            Dictionary<string, string> leftPropValue = new Dictionary<string, string>(left.Count);
            Dictionary<string, string> rightPropValue = new Dictionary<string, string>(right.Count);

            foreach (var element in leftArray) {
                leftPropertyNames.Add(element.Name);
                leftPropValue[element.Name] = element.EvaluatedValue;
            }

            foreach (var element in rightArray) {
                rightPropertyNames.Add(element.Name);
                rightPropValue[element.Name] = element.EvaluatedValue;
            }

            // see if there are any elements in left which are not in right
            var elementsOnlyInLeft = from leftPropName in leftPropertyNames
                                     where !rightPropertyNames.Contains(leftPropName)
                                     select leftPropName;

            var elementsOnlyInright = from rightPropName in rightPropertyNames
                                      where !leftPropertyNames.Contains(rightPropName)
                                      select rightPropName;

            var sharedPropertyNames = from leftPropName in leftPropertyNames
                                      where rightPropertyNames.Contains(leftPropName)
                                      select leftPropName;

            var differentProperties = new List<PropertyDelta>();

            foreach (var name in sharedPropertyNames) {
                var leftValue = leftPropValue[name];
                var rightValue = rightPropValue[name];

                if (!leftValue.Equals(rightValue)) {
                    differentProperties.Add(new PropertyDelta {
                        Name = name,
                        LeftValue = leftValue,
                        RightValue = rightValue
                    });
                }
            }

            bool areEqual = false;
            if (elementsOnlyInLeft.Count() == 0 &&
                elementsOnlyInright.Count() == 0 &&
                differentProperties.Count == 0) {
                areEqual = true;
            }

            var retValue = new PropertyListCompareResult {
                AreEqual = areEqual,
                PropertiesChanged = differentProperties
            };

            foreach (var element in elementsOnlyInLeft) {
                var value = leftPropValue[element];
                retValue.PropertiesOnlyInLeft.Add(
                    new PropertyDelta {
                        Name = element,
                        LeftValue = value
                    });
            }

            foreach (var element in elementsOnlyInright) {
                var value = rightPropValue[element];
                retValue.PropertiesOnlyInRight.Add(
                    new PropertyDelta {
                        Name = element,
                        RightValue = value
                    });
            }

            return retValue;
        }
        private string GetNameFor(ProjectItemInstance item) {
            return string.Format("{0}:{1}", item.ItemType, item.EvaluatedInclude);
        }
        public ItemListCollectionCompareResult Compare(ICollection<ProjectItemInstance> left, ICollection<ProjectItemInstance> right) {
            var leftArray = left.ToArray();
            var rightArray = right.ToArray();

            if (left.SequenceEqual(right, new ProjectItemInstanceComparer())) {
                return new ItemListCollectionCompareResult { AreEqual = true };
            }

            List<string> leftItemNames = new List<string>();
            List<string> rightItemNames = new List<string>();

            var leftItems = new Dictionary<string, ProjectItemInstance>();
            var rightItems = new Dictionary<string, ProjectItemInstance>();
            foreach (var item in left) {
                leftItemNames.Add(GetNameFor(item));
                leftItems[GetNameFor(item)] = item;
            }

            foreach (var item in right) {
                rightItemNames.Add(string.Format("{0}:{1}", item.ItemType, item.EvaluatedInclude));
                rightItems[GetNameFor(item)] = item;
            }

            var result = new ItemListCollectionCompareResult();

            // find new items only in right (i.e. items added)
            
            var elementsOnlyInLeft = from leftItemName in leftItemNames
                                     where !rightItemNames.Contains(leftItemName)
                                     select leftItemName;

            foreach (var name in elementsOnlyInLeft) {
                // result.ItemsOnlyInLeft.Add(leftItems[string.Format("{0}:{1}", item.ItemType, item.EvaluatedInclude)]);
                result.ItemsOnlyInLeft.Add(leftItems[name]);
            }

            var elementsOnlyInRight = from rightItemName in rightItemNames
                                      where !leftItemNames.Contains(rightItemName)
                                      select rightItemName;

            foreach (var name in elementsOnlyInRight) {
                result.ItemsOnlyInRight.Add(rightItems[name]);
            }

            var sharedItemNames = from leftItemName in leftItemNames
                                  where rightItemNames.Contains(leftItemName)
                                  select leftItemName;
            
            // loop through each and find changed values
            foreach (string name in sharedItemNames) {
                var leftItem = leftItems[name];
                var rightItem = rightItems[name];

                if (_piiComparer.Compare(leftItem, rightItem) != 0) {
                    result.ItemsChanged.Add(
                        new Tuple<ProjectItemInstance, ProjectItemInstance>(leftItem, rightItem));
                }
            }

            if(result.ItemsChanged.Count == 0 &&
                result.ItemsOnlyInLeft.Count == 0 &&
                result.ItemsOnlyInRight.Count == 0) {
                    result.AreEqual = true;
            }
            else {
                result.AreEqual = false;
            }

            return result;
        }

        // http://www.dotnetperls.com/sequenceequal
        private bool ArraysEqual(ProjectPropertyInstance[] a1, ProjectPropertyInstance[] a2) {
            if (a1.Length == a2.Length) {
                for (int i = 0; i < a1.Length; i++) {
                    if (a1[i] != a2[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        private bool ArraysEqual(ProjectItemInstance[] a1, ProjectItemInstance[] a2) {
            if (a1.Length == a2.Length) {
                for (int i = 0; i < a1.Length; i++) {
                    if (a1[i] != a2[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }


    public class PropertyListCompareResult {
        public PropertyListCompareResult() {
            PropertiesOnlyInLeft = new List<PropertyDelta>();
            PropertiesOnlyInRight = new List<PropertyDelta>();
            PropertiesChanged = new List<PropertyDelta>();
        }
        public bool AreEqual { get; set; }
        public IList<PropertyDelta> PropertiesOnlyInLeft { get; set; }
        public IList<PropertyDelta> PropertiesOnlyInRight { get; set; }
        public IList<PropertyDelta> PropertiesChanged { get; set; }
    }
    public class PropertyDelta {
        public string Name { get; set; }
        public string LeftValue { get; set; }
        public string RightValue { get; set; }
    }
    public class ItemListDelta {
        public string Name { get; set; }
        public IList<string> LeftValues { get; set; }
        public IList<string> RightValues { get; set; }
    }
    public class ItemListCollectionCompareResult {
        public ItemListCollectionCompareResult() {
            AreEqual = false;
            ItemsOnlyInLeft = new List<ProjectItemInstance>();
            ItemsOnlyInRight = new List<ProjectItemInstance>();
            ItemsChanged = new List<Tuple<ProjectItemInstance, ProjectItemInstance>>();
        }
        public bool AreEqual { get; set; }
        public IList<ProjectItemInstance> ItemsOnlyInLeft { get; set; }
        public IList<ProjectItemInstance> ItemsOnlyInRight { get; set; }
        public IList<Tuple<ProjectItemInstance, ProjectItemInstance>> ItemsChanged { get; set; }
    }
    
    public class ProjectItemInstanceComparer : IComparer<ProjectItemInstance>, IEqualityComparer<ProjectItemInstance> {
        public int Compare(ProjectItemInstance x, ProjectItemInstance y) {
            if (x == null && y != null) { return -1; }
            if (x != null && y == null) { return 1; }
            if (x == null && y == null) { return 1; }

            if (string.Compare(x.ItemType, y.ItemType, StringComparison.OrdinalIgnoreCase) != 0)
                return 1;

            if (x.MetadataCount != y.MetadataCount)
                return 1;

            if (string.Compare(x.EvaluatedInclude, y.EvaluatedInclude, StringComparison.OrdinalIgnoreCase) != 0)
                return 1;

            if (!x.MetadataNames.SequenceEqual(y.MetadataNames))
                return 1;
            
            var mdNameValueX = new Dictionary<string, string>();
            var mdNameValueY = new Dictionary<string, string>();
            foreach (var name in x.MetadataNames) {
                // check that each metadata name has the same value on x and y
                //if (string.Compare(x.SafeGetMetadataValue(name), y.SafeGetMetadataValue(name), StringComparison.OrdinalIgnoreCase) != 0) {
                //    return 1;
                //}
            }

            // they are equal if we get to here
            return 0;
            throw new NotImplementedException();
        }

        public bool Equals(ProjectItemInstance x, ProjectItemInstance y) {
            return this.Compare(x, y) == 0;
        }

        public int GetHashCode(ProjectItemInstance obj) {
            return obj.GetHashCode();
        }
    }
}
