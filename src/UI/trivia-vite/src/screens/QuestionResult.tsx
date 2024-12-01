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


 
export function QuestionResult() {

    const userGameState = useGameStore(s => s.gameParticipantState)

  return (
    <>
     <h1>Fast Trivia</h1>
        <Card>
          <CardHeader>
            <CardTitle>Question {userGameState.game.currentQuestionNumber}</CardTitle>
            <CardDescription>
             {userGameState.currentQuestion}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
         
            TODO: QuestionResultHere!

          </CardContent>
          <CardFooter>
            
          </CardFooter>
        </Card>
     
    </>
    
  )
}
