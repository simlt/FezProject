using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebService.Models;

namespace WebService.Controllers
{
    [RoutePrefix("api/Games")]
    public class GamesController : ApiController
    {
        private WebServiceContext db = new WebServiceContext();

        // GET: api/Games
        public IEnumerable<GameDTO> GetGames()
        {
            return db.Games.AsEnumerable().Select(g => new GameDTO(g));
        }

        // GET: api/Games/5
        [Route("{id:int}", Name = "GetGameFromId")]
        [ResponseType(typeof(GameDTO))]
        public async Task<IHttpActionResult> GetGame(int id)
        {
            Game game = await db.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            return Ok(new GameDTO(game));
        }

        // PUT: api/Games/5
        /*[ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGame(int id, Game game)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != game.GameID)
            {
                return BadRequest();
            }

            db.Entry(game).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(id))
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

        // POST: api/Games
        [ResponseType(typeof(GameDTO))]
        public async Task<IHttpActionResult> PostGame()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate Random item list for game
            // TODO this doesn't store the generated order in the database yet
            const int itemsPerGame = 5;
            var items = db.Items.AsEnumerable().Select(item => new ItemDTO(item));
            var randomItems = db.Items.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(itemsPerGame);
            Game game = new Game();
            game.Items = new List<Item>(randomItems);

            db.Games.Add(game);
            await db.SaveChangesAsync();

            return CreatedAtRoute("GetGameFromId", new { id = game.GameID }, new GameDTO(game));
        }

        // DELETE: api/Games/5
        /*[ResponseType(typeof(Game))]
        public async Task<IHttpActionResult> DeleteGame(int id)
        {
            Game game = await db.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            db.Games.Remove(game);
            await db.SaveChangesAsync();

            return Ok(game);
        }*/

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GameExists(int id)
        {
            return db.Games.Count(e => e.GameID == id) > 0;
        }
    }
}