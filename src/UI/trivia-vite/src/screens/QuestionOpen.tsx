import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import * as RadioGroup from "@radix-ui/react-radio-group";
import { useGameStore } from '../store/GameStore'
import { gql, useSubscription } from "@apollo/client";
import { subscriptionClient } from "@/store/UserGameState";


 
export function QuestionOpen() {

    const userGameState = useGameStore(s => s.gameParticipantState)
    let answerIndex=0;

  return (
    <>
    <h1>Question {userGameState.game.currentQuestionNumber}</h1>
        <Card>
          <CardHeader>
            <CardTitle>Question {userGameState.game.currentQuestionNumber}</CardTitle>
            <CardDescription>
             {userGameState.currentQuestion}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
         
          <RadioGroup.Root>
            { userGameState.answers.map(a => <>
              <RadioGroup.Item value={''+answerIndex++}>{a}</RadioGroup.Item>
            </>) }
            </RadioGroup.Root>

          </CardContent>
          <CardFooter>
            
          </CardFooter>
        </Card>
     
    </>
    
  )
}
