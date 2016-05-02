namespace HelloRPS.Core.Models

type Move =
    | Rock
    | Paper
    | Scissors

type GameResult =
    | PlayerOneWin
    | PlayerTwoWin
    | Tie

type GameState =
    | NotStarted
    | Started
    | Ended

type State =
    { gameId: string
      gameState: GameState
      creatorName: string
      creatorMove: Move }
