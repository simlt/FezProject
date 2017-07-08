using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;

namespace WebService.Models
{
    // The requested item to be searched
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemID { get; set; }
        [Required]
        public string Name { get; set; }
        public string LabelsAsString {
            get { return String.Join(",", _labels); }
            set { _labels = value.Split(',').ToList(); }
        }
        [Required]
        public int Points { get; set; }

        private List<string> _labels = new List<string>();
        public List<string> Labels
        {
            get { return _labels; }
            set { _labels = value; }
        }
    }

    [DataContract(Name="Item")]
    public class ItemDTO
    {
        [DataMember]
        public int ItemID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Points { get; set; }

        internal ItemDTO(Item item)
        {
            ItemID = item.ItemID;
            Name = item.Name;
            Points = item.Points;
        }
    }
}