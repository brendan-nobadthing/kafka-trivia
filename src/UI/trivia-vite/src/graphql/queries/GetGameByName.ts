import { gql } from "@apollo/client";

export const GET_GAME_BY_NAME = gql`
  query getGameByName($name: String!) {
    gameByName(gameName: $name) {
        currentQuestionNumber
        gameId
        timestampUtc
    }
  }
`