namespace MsbuildAnalyzer {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class CommandLineOptionsBuilder {
        public CommandLineOptions GetOptions(string[] args) {
            CommandLineOptions options = new CommandLineOptions();
 
            if (args != null && args.Length >=2){
                if (args[0] != null) options.ProjectFilePath = args[0];
                
                if (args[1] != null) options.LogfilePath = args[1];
            }

            return options;
        }
    }

    internal class CommandLineOptions {
        private bool _isValidationCurrent;
        private bool _areOptionsValid { get; set; }
        private string _projectFilePath{get;set;}

        private string _validationErrorMessage;
        public string ValidationErrorMessage {
            get {
                if (!_isValidationCurrent) {
                }
                return _validationErrorMessage;
            }
            set { this._projectFilePath = value; }
        }

        public bool AreOptionsValid {
            get {
                if (!this._isValidationCurrent) {
                    StringBuilder sb = new StringBuilder();

                    this._areOptionsValid = true;
                    
                    if (string.IsNullOrWhiteSpace(this.ProjectFilePath)) {
                        sb.AppendFormat("ProjectFilePath is null or empty{0}",Environment.NewLine);
                        this._areOptionsValid = false;
                    }
                    if (string.IsNullOrWhiteSpace(this.LogfilePath)) {
                        sb.AppendFormat("LogfilePath is null or empty{0}", Environment.NewLine);
                        this._areOptionsValid = false;
                    }
                    
                    this._isValidationCurrent = true;
                }

                return this._areOptionsValid;
            }
        }

        public string ProjectFilePath {
            get { return this._projectFilePath; }
            set {
                this._projectFilePath = value;
                this._isValidationCurrent = false;
            }
        }
        private string _logFilePath;
        public string LogfilePath {
            get { return _logFilePath; }
            set {
                _logFilePath = value;
                _isValidationCurrent = false;
            }
        }
    }
}
