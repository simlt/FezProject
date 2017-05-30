using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebService.Models
{
    // The item requested to be searched
    public class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemID { get; set; }
        [Required]
        public string Name { get; set; }
        public string LabelsAsString {
            get { return String.Join(",", _labels); }
            set { _labels = value.Split(',').ToList(); }
        }
        // TODO maybe add categories?

        private List<string> _labels = new List<string>();
        public List<string> Labels
        {
            get { return _labels; }
            set { _labels = value; }
        }
    }
}