using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        public byte[] Image { get; set; }
        public Boolean VerificationResult { get; set; }
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

    public class ImageSubmissionDTO
    {
        public int ImageID { get; set; }
        public int ItemID { get; set; }
        public Boolean VerificationResult { get; set; }
        public List<string> Labels { get; set; }

        internal ImageSubmissionDTO(ImageSubmission imageSubmission)
        {
            ImageID = imageSubmission.ImageID;
            ItemID = imageSubmission.ItemID;
            VerificationResult = imageSubmission.VerificationResult;
            Labels = imageSubmission.Labels;
        }
    }
}