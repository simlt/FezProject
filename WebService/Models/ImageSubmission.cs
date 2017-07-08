using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebService.Models
{
    public class ImageSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageID { get; set; }
        /*[Required]
        public string Description { get; set; }*/
        [Required]
        public int GameID { get; set; }
        [Required]
        public byte[] Image { get; set; }
        public bool VerificationResult { get; set; }
        public string LabelsAsString
        {
            get { return String.Join(",", _labels); }
            set { _labels = value.Split(',').ToList(); }
        }
        // Foreign key
        public int ItemID { get; set; }
        // Navigation property (virtual for Lazy Loading)
        public virtual Item Item { get; set; }

        private List<string> _labels = new List<string>();
        public List<string> Labels
        {
            get { return _labels; }
            set { _labels = value; }
        }

    }

    [DataContract(Name = "ImageSubmission")]
    public class ImageSubmissionDTO
    {
        /*[DataMember]
        public int GameID { get; set; }*/
        [DataMember]
        public int ImageID { get; set; }
        [DataMember]
        public int ItemID { get; set; }
        [DataMember]
        public bool VerificationResult { get; set; }
        [DataMember]
        public List<string> Labels { get; set; }

        internal ImageSubmissionDTO(ImageSubmission imageSubmission)
        {
            //GameID = imageSubmission.GameID;
            ImageID = imageSubmission.ImageID;
            ItemID = imageSubmission.ItemID;
            VerificationResult = imageSubmission.VerificationResult;
            Labels = imageSubmission.Labels;
        }
    }
}