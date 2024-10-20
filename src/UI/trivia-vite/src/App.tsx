import { useState } from 'react'
import './App.css'
import { Home } from './screens/Home.tsx'; 

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="dark">
      <Home/>
    </div>
      
  )
}

export default App
