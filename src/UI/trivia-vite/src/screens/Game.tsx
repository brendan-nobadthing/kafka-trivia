import { useGameStore } from '../store/GameStore'
import { gql, useSubscription } from "@apollo/client";
import { subscriptionClient } from "@/store/UserGameState";
import { GameState } from "@/graphql/generated/graphql";
import { QuestionOpen } from './QuestionOpen';
import { QuestionResult } from './QuestionResult';
import { Lobby } from './Lobby';


const GAME_CHANGED = gql`
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



const GAME_PARTICIPANT_STATE_CHANGED = gql`
subscription GameParticipantStateChanged($participantId:UUID!) {
  gameParticipantStateChanged(participantId: $participantId) {
    answers
    currentQuestion
    game {
      currentQuestionNumber
      gameId
      gameState
      name
      timestampUtc
    }
    participant {
      gameId
      gravatarCode
      isOwner
      name
      participantId
    }
  }
}
`


 
export function Game() {

    const userGameState = useGameStore(s => s.gameParticipantState)


    // subscribe to gameParticipantState graphql and push to state   
    useSubscription (GAME_PARTICIPANT_STATE_CHANGED, {
      variables: { 'participantId': userGameState.participant.participantId  },
      client: subscriptionClient,
      errorPolicy: 'all',
      onData({ data }) {
        console.log('GAME PARTICIPANT STATE', data);
      }
    });


  return (
    <>
    {
     (() => {
          if (userGameState.game.gameState == GameState.LobbyOpen) return (<Lobby/>)
          if (userGameState.game.gameState == GameState.QuestionOpen) return (<QuestionOpen/>)
          if (userGameState.game.gameState == GameState.QuestionResult) return (<QuestionResult/>)
        })()
      }
    <div><pre>{JSON.stringify(userGameState, null, 2) }</pre></div>
    </>
     
  )
}
