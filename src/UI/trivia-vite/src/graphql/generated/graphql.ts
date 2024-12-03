/* eslint-disable */
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  /** The `DateTime` scalar represents an ISO-8601 compliant date time type. */
  DateTime: { input: any; output: any; }
  UUID: { input: any; output: any; }
};

export type AnswerQuestionInput = {
  answerIndex: Scalars['Int']['input'];
  gameId: Scalars['UUID']['input'];
  participantId: Scalars['UUID']['input'];
  questionNumber: Scalars['Int']['input'];
  timestampUtc?: InputMaybe<Scalars['DateTime']['input']>;
};

export type BoolResponse = {
  __typename?: 'BoolResponse';
  isSuccessful: Scalars['Boolean']['output'];
  message: Scalars['String']['output'];
};

export type Game = {
  __typename?: 'Game';
  currentQuestionNumber?: Maybe<Scalars['Int']['output']>;
  currentQuestionStats?: Maybe<Scalars['String']['output']>;
  gameId: Scalars['UUID']['output'];
  gameState: GameState;
  gameStateTimestampUtc?: Maybe<Scalars['DateTime']['output']>;
  name: Scalars['String']['output'];
  timestampUtc: Scalars['DateTime']['output'];
};

export type GameParticipant = {
  __typename?: 'GameParticipant';
  gameId: Scalars['UUID']['output'];
  gravatarCode: Scalars['String']['output'];
  isOwner: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
  participantId: Scalars['UUID']['output'];
};

export type GameParticipantAnswerScore = {
  __typename?: 'GameParticipantAnswerScore';
  answerIndex?: Maybe<Scalars['Int']['output']>;
  correctAnswerIndex: Scalars['Int']['output'];
  gameId: Scalars['UUID']['output'];
  participantId: Scalars['UUID']['output'];
  questionNumber: Scalars['Int']['output'];
  score: Scalars['Int']['output'];
};

export type GameParticipantState = {
  __typename?: 'GameParticipantState';
  answers: Array<Scalars['String']['output']>;
  currentQuestion: Scalars['String']['output'];
  game: Game;
  participant: GameParticipant;
  scores?: Maybe<Array<GameParticipantAnswerScore>>;
};

export type GameParticipants = {
  __typename?: 'GameParticipants';
  gameId: Scalars['UUID']['output'];
  participants: Array<GameParticipant>;
};

export enum GameState {
  Finished = 'FINISHED',
  LobbyOpen = 'LOBBY_OPEN',
  QuestionOpen = 'QUESTION_OPEN',
  QuestionResult = 'QUESTION_RESULT'
}

export type Mutation = {
  __typename?: 'Mutation';
  addParticipant: GameParticipant;
  answerQuestion: BoolResponse;
  newGame: GameParticipant;
  startGame: StartGameResponse;
};


export type MutationAddParticipantArgs = {
  displayName: Scalars['String']['input'];
  email: Scalars['String']['input'];
  gameName: Scalars['String']['input'];
};


export type MutationAnswerQuestionArgs = {
  answer: AnswerQuestionInput;
};


export type MutationNewGameArgs = {
  name: Scalars['String']['input'];
};


export type MutationStartGameArgs = {
  gameId: Scalars['UUID']['input'];
};

export type Query = {
  __typename?: 'Query';
  game?: Maybe<Game>;
  gameByName?: Maybe<Game>;
};


export type QueryGameArgs = {
  gameId: Scalars['UUID']['input'];
};


export type QueryGameByNameArgs = {
  gameName: Scalars['String']['input'];
};

export type StartGameResponse = {
  __typename?: 'StartGameResponse';
  gameId: Scalars['UUID']['output'];
};

export type Subscription = {
  __typename?: 'Subscription';
  gameParticipantStateChanged: GameParticipantState;
  gameParticipantsChanged: GameParticipants;
};


export type SubscriptionGameParticipantStateChangedArgs = {
  participantId: Scalars['UUID']['input'];
};


export type SubscriptionGameParticipantsChangedArgs = {
  gameId: Scalars['UUID']['input'];
};
