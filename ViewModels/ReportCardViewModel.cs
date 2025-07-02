using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public partial class ReportCardViewModel : BaseViewModel
    {

        private Report _report;


        public Report Report
        {
            get => _report;
            set
            {
                if (_report != value)
                {
                    _report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }
        public ReportCardViewModel() { }

        public ReportCardViewModel(Report report)
        {
            Report = report;
        }
    }
}
