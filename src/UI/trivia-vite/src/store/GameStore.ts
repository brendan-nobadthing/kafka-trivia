import { create } from 'zustand'
//import { devtools, persist } from 'zustand/middleware'
import { immer } from 'zustand/middleware/immer'
import type {} from '@redux-devtools/extension' // required for devtools typing
import { NewOrJoinGameSlice, createNewOrJoinGameSlice } from './NewOrJoinGame';
import { GameSlice, createGameSlice } from './UserGameState';



export const useGameStore = create<NewOrJoinGameSlice & GameSlice>()(immer((...a) => ({
    ...createNewOrJoinGameSlice(...a),
    ...createGameSlice(...a),
  })));


  