import { create } from 'zustand'
//import { devtools, persist } from 'zustand/middleware'
import { immer } from 'zustand/middleware/immer'
import type {} from '@redux-devtools/extension' // required for devtools typing

enum GameUserRole {
    Owner,
    Player
}

interface JoinGame {
    name: string,
    isAvailable: boolean,
    error: string
}

interface NewGame {
    name: string,
    isAvailable : boolean,
    error: string | null
}

interface Game {
    gameId: string,
}

interface GameState {
  newGame: NewGame,
  checkNewGame: (name: string) => void
}

export const useGameStore = create<GameState>()(
    immer((set) => ({
        newGame: { name: '', isAvailable: false, error: null },
        checkNewGame: (n: string) => set((s) => { 
            s.newGame.name = n;
            s.isAvailable = true;
        })
      }))
)