import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { useGameStore } from '../store/GameStore'
import { gql, useSubscription } from "@apollo/client";
import { subscriptionClient } from "@/store/UserGameState";


const GAME_PARTICIPANTS_CHANGED = gql`
subscription participants ($gameId: UUID!) {
  gameParticipantsChanged(gameId: $gameId) {
    gameId
    participants {
      gameId
      gravatarCode
      name
      participantId
    }
  }
}
`;
 
export function Lobby() {

    const userGameState = useGameStore(s => s.gameParticipantState)
    const startGame = useGameStore(s => s.startGame)

    const { data, loading } = useSubscription(
        GAME_PARTICIPANTS_CHANGED,
        { 
            variables: { gameId: userGameState.game.gameId },
            client: subscriptionClient,
        },
    );


    const showParticipants = !loading && data.gameParticipantsChanged.participants.map((p:any,i:number) => 
        <li key={i}>{p.name}</li>
    );

  return (
    <>
    <h1>Fast Trivia</h1>
        <Card>
          <CardHeader>
            <CardTitle>Game Lobby: {userGameState.game.name}</CardTitle>
            <CardDescription>
              Names will appear below as they join the game
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            gameid: { userGameState.game.name }<br/>
            <div><ul>{showParticipants}</ul></div>
           {/*  <div><pre>{JSON.stringify(data, null, 2) }</pre></div>*/}

          </CardContent>
          <CardFooter>
            <Button 
              disabled={!userGameState.participant.isOwner} 
              onClick={() => startGame(userGameState.game.gameId)}
              >Start Game</Button>
          </CardFooter>
        </Card>
        <div><pre>{JSON.stringify(userGameState, null, 2) }</pre></div>
    </>
    
  )
}
