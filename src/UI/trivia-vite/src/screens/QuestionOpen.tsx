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
    const answerQuestion = useGameStore(s => s.answerQuestion)

    if (!userGameState.currentQuestion) return (<><h1>Starting...</h1></>)

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
         
          <RadioGroup.Root className="flex space-x-2" onValueChange={ (v) => answerQuestion( { 
                answerIndex: parseInt(v), 
                gameId: userGameState.game.gameId, 
                participantId: userGameState.participant.participantId, 
                questionNumber: userGameState.game.currentQuestionNumber! 
              }) } >
            { userGameState.answers.map((a,i) => <>
              <RadioGroup.Item value={''+i}>{a}</RadioGroup.Item>
            </>) }
            </RadioGroup.Root>

          </CardContent>
          <CardFooter>
            { userGameState.game?.currentQuestionStats && `${userGameState.game?.currentQuestionStats} Answers Received` }
          </CardFooter>
        </Card>
     
    </>
    
  )
}
