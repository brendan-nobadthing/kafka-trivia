import { StateCreator } from 'zustand'
import { NewOrJoinGameSlice } from './NewOrJoinGame'



export interface UserGameState {
    gameId: string,
    participantId: string,
    gameState: string,
    questionNumber: number | null
}

export interface GameSlice {
   userGameState: UserGameState,
   update: (userGameState: UserGameState) => void
    
}

export const createGameSlice: StateCreator<GameSlice & NewOrJoinGameSlice,[["zustand/immer", never], never],[],GameSlice> = (set) => ({
   userGameState: {
    gameId: '',
    participantId: '',
    gameState: '',
    questionNumber: null
   },
   update: (newState: UserGameState) => set((state: GameSlice) => {
    state.userGameState = newState;
   }),
});
