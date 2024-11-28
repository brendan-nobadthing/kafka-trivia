import { StateCreator } from 'zustand'
import { GameSlice } from './UserGameState'
import { ApolloClient, InMemoryCache } from '@apollo/client'
import { gql } from "@apollo/client";



export interface NewOrJoinGameState {
    isNewGame: boolean,
    name: string,
    isAvailable : boolean,
    error: string | null
}

export interface NewOrJoinGameSlice {
    newOrJoinState: NewOrJoinGameState,
    checkGame: (name: string, isNewGame: boolean) => void
}

export const createNewOrJoinGameSlice: StateCreator<GameSlice & NewOrJoinGameSlice,[["zustand/immer", never], never],[],NewOrJoinGameSlice> = (set) => ({
    newOrJoinState: {
        isNewGame: false,
        name: '',
        isAvailable : false,
        error: null
    },
    checkGame: async (name: string, isNewGame: boolean) => {
        console.log('CHECK GAME', name);
        if (name.length < 3) {
            set((state: NewOrJoinGameSlice) => {
                state.newOrJoinState.name = name;
                state.newOrJoinState.isNewGame = isNewGame;
                state.newOrJoinState.isAvailable = false;
            });
            return;
        }
        const result = await apolloClient.query({
                query: GET_GAME_BY_NAME ,
                variables: { name: name },
                errorPolicy: 'all'
                
        });
        console.log("CHECK RESULT", result);
        
        set((state: NewOrJoinGameSlice) => { 
            state.newOrJoinState.name = name;
            state.newOrJoinState.isNewGame = isNewGame;
            const isAvailable = (isNewGame && !result.data.gameByName) || (!isNewGame && !!result.data.gameByName);
            state.newOrJoinState.isAvailable = isAvailable,
            state.newOrJoinState.error = !isAvailable 
                ? isNewGame ? "Game Already Exists" : "Game Not Found"
                : null; 
        });
    }
});



const apolloClient = new ApolloClient({
    uri: "https://localhost:7062/graphql",
    cache: new InMemoryCache()
});


const GET_GAME_BY_NAME = gql`
  query getGameByName($name: String!) {
    gameByName(gameName: $name) {
        currentQuestionNumber
        gameId
        timestampUtc
    }
  }
`
