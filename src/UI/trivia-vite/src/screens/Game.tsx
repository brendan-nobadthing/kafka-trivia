import { useGameStore } from '../store/GameStore'
import { gql, useSubscription } from "@apollo/client";
import { subscriptionClient } from "@/store/UserGameState";
import { GameParticipantState, GameState } from "@/graphql/generated/graphql";
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
      isOwner
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
      gameStateTimestampUtc
      currentQuestionStats
    }
    participant {
      gameId
      gravatarCode
      isOwner
      name
      participantId
    }
    scores {
      answerIndex
      gameId
      participantId
      questionNumber
      score
      correctAnswerIndex
    }
  }
}
`

 
export function Game() {

    const userGameState = useGameStore(s => s.gameParticipantState)
    const updateGameState = useGameStore(s => s.update)

    // subscribe to gameParticipantState graphql and push to state   
    useSubscription (GAME_PARTICIPANT_STATE_CHANGED, {
      variables: { 'participantId': userGameState.participant.participantId  },
      client: subscriptionClient,
      errorPolicy: 'all',
      onData({ data }) {
        var newState: GameParticipantState =  data.data.gameParticipantStateChanged;
        console.log('GAME PARTICIPANT STATE', newState);
        updateGameState(newState);
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
     {userGameState.participant.participantId}
    </>
     
  )
}
