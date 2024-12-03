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

    let currentScore = userGameState.scores?.find(s => s.questionNumber == userGameState.game.currentQuestionNumber);
    let isCorrect = currentScore && currentScore.answerIndex == currentScore?.correctAnswerIndex
    let yourAnswer =  currentScore?.answerIndex == null ? 'with nothing!' : userGameState.answers[currentScore.answerIndex] ;
    let correctAnswer = currentScore? userGameState.answers[currentScore.correctAnswerIndex] : null; 
    
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
         
              <h2>{isCorrect ? 'Correct!':'Wrong!' }</h2>
              You Answered: { yourAnswer }<br/>
              { currentScore && !isCorrect ? <>Correct Answer: {correctAnswer}<br/></> : "" }
              Score: { currentScore ? currentScore.score : 0 }
          
          </CardContent>
          <CardFooter>
            
          </CardFooter>
        </Card>
     
    </>
    
  )
}
