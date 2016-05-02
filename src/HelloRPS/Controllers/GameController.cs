using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using HelloRPS.Core;
using HelloRPS.Core.Models;

namespace HelloRPS.Controllers
{
    [RoutePrefix("api/game")]
    public class GameController : ApiController
    {
        private static readonly IDictionary<string, State> Games
            = new ConcurrentDictionary<string, State>();
        private static readonly IDictionary<string, string> Outcomes
            = new ConcurrentDictionary<string, string>();

        [HttpPost]
        [Route("create")]
        public IHttpActionResult Create()
        {
            var gameId = Guid.NewGuid().ToString();
            Games[gameId] = Game.EmptyState;

            return Created(Url.Link("Move", new { gameId }), String.Empty);
        }

        [HttpPut]
        [Route("{gameId}", Name = "Move")]
        public IHttpActionResult Move(
            string gameId,
            [FromBody] Models.MoveRequest move)
        {
            if (!Games.ContainsKey(gameId)) return NotFound();

            Move m = null;
            if (!Interop.TryParseStateFromString(move.Move, out m))
            {
                return BadRequest("Invalid move");
            }

            var game = Games[gameId];
            if (game == Game.EmptyState)
            {
                Games[gameId] = new State(gameId, GameState.Started, move.PlayerName, m);
                var response = new HttpResponseMessage(HttpStatusCode.Accepted);
                response.Headers.Location = new Uri(Url.Link("Status", new { gameId }));

                return ResponseMessage(response);
            }

            if (game.creatorName?.Equals(move.PlayerName, StringComparison.Ordinal) ?? false) return BadRequest("Player has already moved");

            game = new State(gameId, GameState.Ended, game.creatorName, game.creatorMove);

            var outcomeMessage = Outcome(game.creatorMove, game.creatorName, m, move.PlayerName);
            Outcomes[gameId] = outcomeMessage;

            return Ok(outcomeMessage);
        }

        private static string Outcome(Move p1Move, string p1Name, Move p2Move, string p2Name)
        {
            var outcome = Game.Outcome(p1Move, p2Move);
            if (outcome.IsPlayerOneWin) return p1Name + " won.";
            else if (outcome.IsPlayerTwoWin) return p2Name + " won.";
            else return "The game ended with a tie.";
        }

        [HttpGet]
        [Route("{gameId}", Name = "Status")]
        public IHttpActionResult Status(string gameId)
        {
            if (!Games.ContainsKey(gameId)) return NotFound();
            if (!(Games[gameId].gameState == Core.Models.GameState.Ended
                || Outcomes.ContainsKey(gameId))) return Ok("Awaiting other player's move.");

            return Ok(Outcomes[gameId]);
        }
    }
}
