import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
//import * as RadioGroup from "@radix-ui/react-radio-group";
import { useGameStore } from '../store/GameStore'
import { useState } from 'react';
//import { gql, useSubscription } from "@apollo/client";
//import { subscriptionClient } from "@/store/UserGameState";


 
export function QuestionOpen() {

    const userGameState = useGameStore(s => s.gameParticipantState)
    const answerQuestion = useGameStore(s => s.answerQuestion)

    const[answer, setAnswer] = useState<number>(-1)

    if (!userGameState.currentQuestion) return (<><h1>Starting...</h1></>)

  return (
    <>
    <h1>Fast Trivia</h1>
        <Card>
          <CardHeader>
            <CardTitle>Question {userGameState.game.currentQuestionNumber}</CardTitle>
            <CardDescription className="text-xl">
             {userGameState.currentQuestion}
            </CardDescription>
          </CardHeader>
          <CardContent className="flex space-x-2 justify-evenly">

         
          {/* <RadioGroup.Root className="flex space-x-2" onValueChange={ (v) => answerQuestion( { 
                answerIndex: parseInt(v), 
                gameId: userGameState.game.gameId, 
                participantId: userGameState.participant.participantId, 
                questionNumber: userGameState.game.currentQuestionNumber! 
              }) } >
            { userGameState.answers.map((a,i) => <>
              <RadioGroup.Item value={''+i}>{a}</RadioGroup.Item>
            </>) }
            </RadioGroup.Root> */}

            { userGameState.answers.map((a,i) => <>
              <Button className="text-wrap min-h-24 min-w-40"
                  disabled={answer > -1}
                  variant={ answer==i ? "destructive" : "default" }
                  onClick={ () => {
                    setAnswer(i)
                    answerQuestion( { 
                    answerIndex: i, 
                    gameId: userGameState.game.gameId, 
                    participantId: userGameState.participant.participantId, 
                    questionNumber: userGameState.game.currentQuestionNumber! 
                  }) 
                }}
              >{a}</Button>
            </>) }



          </CardContent>
          <CardFooter>
            { userGameState.game?.currentQuestionStats && `${userGameState.game?.currentQuestionStats} Answers Received` }
          </CardFooter>
        </Card>
     
    </>
    
  )
}
