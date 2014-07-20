namespace MsbuildAnalyzer.Common {
    using Microsoft.Build.Execution;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MSBuildComparer {
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

            leftPropValue.SequenceEqual(rightPropValue);

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
            if(elementsOnlyInLeft.Count() == 0 && 
                elementsOnlyInright.Count() == 0 &&
                differentProperties.Count == 0) {
                    areEqual = true;
            }

            var retValue = new PropertyListCompareResult {
                AreEqual= areEqual,
                ChangedProperties = differentProperties
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

    }


    public class PropertyListCompareResult {
        public PropertyListCompareResult() {
            PropertiesOnlyInLeft = new List<PropertyDelta>();
            PropertiesOnlyInRight = new List<PropertyDelta>();
            ChangedProperties = new List<PropertyDelta>();
        }
        public bool AreEqual { get; set; }
        public IList<PropertyDelta> PropertiesOnlyInLeft { get; set; }
        public IList<PropertyDelta> PropertiesOnlyInRight { get; set; }
        public IList<PropertyDelta> ChangedProperties { get; set; }
    }
    public class PropertyDelta {
        public string Name { get; set; }
        public string LeftValue { get; set; }
        public string RightValue { get; set; }
    }
}
