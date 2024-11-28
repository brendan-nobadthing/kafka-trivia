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


 
export function GameFinished() {

    const userGameState = useGameStore(s => s.gameParticipantState)

  return (
    <>
    <h1>Game Finished</h1>
        <Card>
          <CardHeader>
            <CardTitle></CardTitle>
            <CardDescription>
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
         
            Game Finished!

          </CardContent>
          <CardFooter>
            
          </CardFooter>
        </Card>
     
    </>
    
  )
}
