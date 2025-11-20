using System.Collections.Generic;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ReportsIndexViewModelTests
    {
        [Fact]
        public void DraftCount_ReturnsCorrectNumber()
        {
            var vm = new ReportsIndexViewModel
            {
                Reports = new List<ReportItem>
                {
                    new ReportItem { IsDraft = true,  PilotID = 1 },
                    new ReportItem { IsDraft = false, PilotID = 1 },
                    new ReportItem { IsDraft = true,  PilotID = 1 }
                }
            };

            Assert.Equal(2, vm.DraftCount);
        }

        [Fact]
        public void SubmittedCount_ReturnsCorrectNumber()
        {
            var vm = new ReportsIndexViewModel
            {
                Reports = new List<ReportItem>
                {
                    new ReportItem { IsDraft = true,  PilotID = 1 },
                    new ReportItem { IsDraft = false, PilotID = 1 },
                    new ReportItem { IsDraft = false, PilotID = 1 }
                }
            };

            Assert.Equal(2, vm.SubmittedCount);
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var vm = new ReportsIndexViewModel();

            Assert.Equal("all", vm.Status);
            Assert.Equal("date_desc", vm.Sort);
            Assert.Equal(string.Empty, vm.Organization);
            Assert.Empty(vm.Reports);
            Assert.Equal(0, vm.TotalCount);
            Assert.Equal(0, vm.FilteredCount);
        }
    }
}