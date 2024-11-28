import './App.css'
import { GameState } from './graphql/generated/graphql.ts';
import { Game } from './screens/Game.tsx';
import { GameFinished } from './screens/GameFinished.tsx';
import { Home } from './screens/Home.tsx'; 
import { Lobby } from './screens/Lobby.tsx'; 
import { useGameStore } from './store/GameStore.ts';

function App() {

  const userGameState = useGameStore(s => s.gameParticipantState);

  return (
    <div className="dark">
      {
        (() => {
          if (!userGameState.game.gameId) return (<Home/>)
          if (userGameState.game.gameState == GameState.Finished) return (<GameFinished/>)
          if (userGameState.game.gameState == GameState.LobbyOpen) return (<Lobby/>)
          return (<Game />)
        })()
      }
    </div>
      
  )
}




export default App
