import { StateCreator } from 'zustand'
import { NewOrJoinGameSlice } from './NewOrJoinGame'
import { ApolloClient, gql, InMemoryCache } from '@apollo/client'
import { GraphQLWsLink } from '@apollo/client/link/subscriptions';
import { createClient } from 'graphql-ws';
import { GameParticipant, GameParticipantState, GameState } from '@/graphql/generated/graphql';


export interface GameSlice {
   gameParticipantState: GameParticipantState
   newGame : (name: string) => void,
   joinGame : (gameName: string, diaplayName: string, email: string) => void,
   startGame: (gameId: string) => void
}

export const createGameSlice: StateCreator<GameSlice & NewOrJoinGameSlice,[["zustand/immer", never], never],[],GameSlice> = (set) => ({
   gameParticipantState: {
        answers: [],
        currentQuestion: '',
        game: {
            gameId: '',
            gameState: GameState.LobbyOpen,
            name: '',
            currentQuestionNumber: null,
            timestampUtc: null
        },
        participant: {
            gameId: '',
            gravatarCode: '',
            participantId: '',
            name: '',
            isOwner: false
        }
    },


   newGame: async (name: string) => {
      console.log('NEW GAME', name);
      const result = await apolloClient.mutate({
        mutation: NEW_GAME,
        variables: { name: name },
        errorPolicy: 'all'  
      });
      console.log('NEW GAME RESULT', result);
      const newGameResult: GameParticipant = result.data.newGame;
      set((state: GameSlice) => {
        state.gameParticipantState.participant.participantId = newGameResult.participantId;
        state.gameParticipantState.participant.name = newGameResult.name;
        state.gameParticipantState.participant.isOwner = newGameResult.isOwner;
        state.gameParticipantState.game.gameId = newGameResult.gameId;
        state.gameParticipantState.game.name = name;
        state.gameParticipantState.game.gameState = GameState.LobbyOpen; 
      });
   },


   joinGame: async (gameName: string, displayName: string, email: string) => {
        console.log('JOIN GAME');
        const result = await apolloClient.mutate({
            mutation: JOIN_GAME,
            variables: { gameName: gameName, displayName: displayName, email: email  }  
        });
        console.log('JOIN GAME RESULT', result);
        const addParticipantResult: GameParticipant = result.data.addParticipant;
        set((state: GameSlice) => {
            state.gameParticipantState.participant.participantId = addParticipantResult.participantId;
            state.gameParticipantState.participant.name = addParticipantResult.name;
            state.gameParticipantState.participant.isOwner = addParticipantResult.isOwner;
            state.gameParticipantState.game.gameId = addParticipantResult.gameId;
            state.gameParticipantState.game.name = gameName;
            state.gameParticipantState.game.gameState = GameState.LobbyOpen; 
        });
    },

    // might not need to be a state method
    startGame: async (gameId: string) => {
        console.log('START GAME');
        const result = await apolloClient.mutate({
            mutation: START_GAME,
            variables: { gameId: gameId  }  
        });
        console.log('START GAME RESULT', result);
        set((state: GameSlice) => {
            // just bump the state here. 
            state.gameParticipantState.game.gameState = GameState.QuestionOpen;
        });
    },

});


const NEW_GAME = gql`
mutation newGame ($name: String!) {
    newGame(name: $name){
      gameId
      gravatarCode
      name
      participantId
      isOwner
    }
}
`

const JOIN_GAME = gql`
mutation addParticipant($gameName: String!, $displayName: String!, $email: String!) {
  addParticipant(gameName: $gameName, displayName: $displayName, email: $email) {
    gameId
    gravatarCode
    name
    participantId
    isOwner
  }
}
`

const START_GAME = gql`
mutation startGame($gameId: UUID!) {
  startGame(gameId: $gameId) {
    gameId
  }
}
`

export const apolloClient = new ApolloClient({
    uri: "https://localhost:7062/graphql",
    cache: new InMemoryCache()
});


const wsLink = new GraphQLWsLink(createClient({
    url: 'wss://localhost:7062/graphql',
  }));


  export const subscriptionClient = new ApolloClient({
    link: wsLink,
    cache: new InMemoryCache()
  });
