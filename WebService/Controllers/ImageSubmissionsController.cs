using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebService.Models;

namespace WebService.Controllers
{
    [RoutePrefix("api/ImageSubmissions")]
    public class ImageSubmissionsController : ApiController
    {
        private WebServiceContext db = new WebServiceContext();

        // GET: api/ImageSubmissions
        public IEnumerable<ImageSubmissionDTO> GetImageSubmissions()
        {
            return db.ImageSubmissions.AsEnumerable().Select(i => new ImageSubmissionDTO(i));
        }

        // GET: api/ImageSubmissions/5
        [Route("{id:int}", Name = "GetImageSubmissionFromId")]
        [ResponseType(typeof(ImageSubmissionDTO))]
        public async Task<IHttpActionResult> GetImageSubmission(int id)
        {
            var imageSubmission = await db.ImageSubmissions.ToAsyncEnumerable().Select(i => new ImageSubmissionDTO(i)).SingleOrDefault(i => i.ImageID == id);
            if (imageSubmission == null)
            {
                return NotFound();
            }

            return Ok(imageSubmission);
        }

        // PUT: api/ImageSubmissions/5
        /*[ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutImageSubmission(int id, ImageSubmission imageSubmission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != imageSubmission.ImageID)
            {
                return BadRequest();
            }

            db.Entry(imageSubmission).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageSubmissionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }*/

        // POST: api/ImageSubmissions
        /*[ResponseType(typeof(ImageSubmission))]
        public async Task<IHttpActionResult> PostImageSubmission(ImageSubmission imageSubmission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // query Google Cloud Service
            // Install with: Install-Package Google.Cloud.Vision.V1 -Pre
            var image = Image.FromBytes(imageSubmission.Image);
            var client = ImageAnnotatorClient.Create();
            var response = client.DetectLabels(image);
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                {
                    Console.WriteLine(annotation.Description);
                    imageSubmission.Labels.Add(annotation.Description);
                }
            }

            db.ImageSubmissions.Add(imageSubmission);
            await db.SaveChangesAsync();
            return CreatedAtRoute("FezAPI", new { id = imageSubmission.ImageID }, imageSubmission);
        }*/

        [Route("~/api/Items/{ItemId}/Submit")]
        [ResponseType(typeof(ImageSubmissionDTO))]
        public async Task<IHttpActionResult> Submit(int ItemId)
        {
            Item item = null;
            try
            {
                item = db.Items.Single(i => i.ItemID == ItemId);
            }
            catch
            {
                return BadRequest("Can't upload image for a non-existing item.");
            }
            if (Request.Content.Headers.ContentType.MediaType != "image/bmp")
            {
                return Content(HttpStatusCode.UnsupportedMediaType, "Only bitmap images are supported.");
            }

            var imageSubmission = new ImageSubmission();
            imageSubmission.ItemID = ItemId;
            imageSubmission.Item = item;
            imageSubmission.Image = await Request.Content.ReadAsByteArrayAsync();
            //var image = Image.FromStream(data.GetStream(Request.Content, Request.Content.Headers));
            var image = Image.FromBytes(imageSubmission.Image);

            // ### QUERY Google Cloud Service
            try
            {
                var client = ImageAnnotatorClient.Create();
                var response = client.DetectLabels(image);
                foreach (var annotation in response)
                {
                    if (annotation.Description != null)
                    {
                        Console.WriteLine(annotation.Description);
                        // Add label to image
                        imageSubmission.Labels.Add(annotation.Description);
                        // Check if label if found on referenced item
                        if (imageSubmission.Item.Labels.Contains(annotation.Description))
                            imageSubmission.VerificationResult = true;
                    }
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception("Could not successfully parse the image on Google Cloud.\n" + e.Message));
            }

            // Save to DB
            db.ImageSubmissions.Add(imageSubmission);
            await db.SaveChangesAsync();

            var dto = new ImageSubmissionDTO(imageSubmission);

            // Return true if the image matches a label
            return CreatedAtRoute("GetImageSubmissionFromId", new { id = imageSubmission.ImageID }, dto);
        }

        // DELETE: api/ImageSubmissions/5
        [ResponseType(typeof(ImageSubmission))]
        public async Task<IHttpActionResult> DeleteImageSubmission(int id)
        {
            ImageSubmission imageSubmission = await db.ImageSubmissions.FindAsync(id);
            if (imageSubmission == null)
            {
                return NotFound();
            }

            db.ImageSubmissions.Remove(imageSubmission);
            await db.SaveChangesAsync();

            return Ok(imageSubmission);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ImageSubmissionExists(int id)
        {
            return db.ImageSubmissions.Count(e => e.ImageID == id) > 0;
        }
    }
}