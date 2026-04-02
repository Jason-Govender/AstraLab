import type { AIConversation, AIResponse } from "@/types/datasets";
import type { MlExperiment } from "@/types/ml";
import type { IDatasetAiAssistantStateContext } from "./context";

type DatasetAiAssistantStatePatch = Partial<IDatasetAiAssistantStateContext>;

export enum DatasetAiAssistantActionEnums {
  loadExperimentContextPending = "DATASET_AI_ASSISTANT_LOAD_EXPERIMENT_CONTEXT_PENDING",
  loadExperimentContextSuccess = "DATASET_AI_ASSISTANT_LOAD_EXPERIMENT_CONTEXT_SUCCESS",
  loadExperimentContextError = "DATASET_AI_ASSISTANT_LOAD_EXPERIMENT_CONTEXT_ERROR",
  getConversationsPending = "DATASET_AI_ASSISTANT_GET_CONVERSATIONS_PENDING",
  getConversationsSuccess = "DATASET_AI_ASSISTANT_GET_CONVERSATIONS_SUCCESS",
  getConversationsError = "DATASET_AI_ASSISTANT_GET_CONVERSATIONS_ERROR",
  getResponsesPending = "DATASET_AI_ASSISTANT_GET_RESPONSES_PENDING",
  getResponsesSuccess = "DATASET_AI_ASSISTANT_GET_RESPONSES_SUCCESS",
  getResponsesError = "DATASET_AI_ASSISTANT_GET_RESPONSES_ERROR",
  generatePending = "DATASET_AI_ASSISTANT_GENERATE_PENDING",
  generateSuccess = "DATASET_AI_ASSISTANT_GENERATE_SUCCESS",
  generateError = "DATASET_AI_ASSISTANT_GENERATE_ERROR",
  setActiveConversation = "DATASET_AI_ASSISTANT_SET_ACTIVE_CONVERSATION",
  clearConversationState = "DATASET_AI_ASSISTANT_CLEAR",
}

interface GetConversationsPendingAction {
  type: DatasetAiAssistantActionEnums.getConversationsPending;
  payload: DatasetAiAssistantStatePatch;
}

interface LoadExperimentContextPendingAction {
  type: DatasetAiAssistantActionEnums.loadExperimentContextPending;
  payload: DatasetAiAssistantStatePatch;
}

interface LoadExperimentContextSuccessAction {
  type: DatasetAiAssistantActionEnums.loadExperimentContextSuccess;
  payload: DatasetAiAssistantStatePatch;
}

interface LoadExperimentContextErrorAction {
  type: DatasetAiAssistantActionEnums.loadExperimentContextError;
  payload: DatasetAiAssistantStatePatch;
}

interface GetConversationsSuccessAction {
  type: DatasetAiAssistantActionEnums.getConversationsSuccess;
  payload: DatasetAiAssistantStatePatch;
}

interface GetConversationsErrorAction {
  type: DatasetAiAssistantActionEnums.getConversationsError;
  payload: DatasetAiAssistantStatePatch;
}

interface GetResponsesPendingAction {
  type: DatasetAiAssistantActionEnums.getResponsesPending;
  payload: DatasetAiAssistantStatePatch;
}

interface GetResponsesSuccessAction {
  type: DatasetAiAssistantActionEnums.getResponsesSuccess;
  payload: DatasetAiAssistantStatePatch;
}

interface GetResponsesErrorAction {
  type: DatasetAiAssistantActionEnums.getResponsesError;
  payload: DatasetAiAssistantStatePatch;
}

interface GeneratePendingAction {
  type: DatasetAiAssistantActionEnums.generatePending;
  payload: DatasetAiAssistantStatePatch;
}

interface GenerateSuccessAction {
  type: DatasetAiAssistantActionEnums.generateSuccess;
  payload: DatasetAiAssistantStatePatch;
}

interface GenerateErrorAction {
  type: DatasetAiAssistantActionEnums.generateError;
  payload: DatasetAiAssistantStatePatch;
}

interface SetActiveConversationAction {
  type: DatasetAiAssistantActionEnums.setActiveConversation;
  payload: DatasetAiAssistantStatePatch;
}

interface ClearConversationStateAction {
  type: DatasetAiAssistantActionEnums.clearConversationState;
  payload: DatasetAiAssistantStatePatch;
}

export type DatasetAiAssistantAction =
  | LoadExperimentContextPendingAction
  | LoadExperimentContextSuccessAction
  | LoadExperimentContextErrorAction
  | GetConversationsPendingAction
  | GetConversationsSuccessAction
  | GetConversationsErrorAction
  | GetResponsesPendingAction
  | GetResponsesSuccessAction
  | GetResponsesErrorAction
  | GeneratePendingAction
  | GenerateSuccessAction
  | GenerateErrorAction
  | SetActiveConversationAction
  | ClearConversationStateAction;

export const loadExperimentContextPending = ({
  experimentId,
}: {
  experimentId?: number;
}): LoadExperimentContextPendingAction => ({
  type: DatasetAiAssistantActionEnums.loadExperimentContextPending,
  payload: {
    activeMlExperimentId: experimentId,
    activeExperiment: undefined,
    isLoadingExperimentContext: Boolean(experimentId),
    isPending: Boolean(experimentId),
    isError: false,
    experimentContextErrorMessage: undefined,
  },
});

export const loadExperimentContextSuccess = ({
  experiment,
}: {
  experiment?: MlExperiment;
}): LoadExperimentContextSuccessAction => ({
  type: DatasetAiAssistantActionEnums.loadExperimentContextSuccess,
  payload: {
    activeMlExperimentId: experiment?.id,
    activeExperiment: experiment,
    isLoadingExperimentContext: false,
    isPending: false,
    isError: false,
    experimentContextErrorMessage: undefined,
  },
});

export const loadExperimentContextError = ({
  experimentId,
  errorMessage,
}: {
  experimentId?: number;
  errorMessage: string;
}): LoadExperimentContextErrorAction => ({
  type: DatasetAiAssistantActionEnums.loadExperimentContextError,
  payload: {
    activeMlExperimentId: experimentId,
    activeExperiment: undefined,
    isLoadingExperimentContext: false,
    isPending: false,
    isError: true,
    experimentContextErrorMessage: errorMessage,
  },
});

export const getConversationsPending = ({
  datasetId,
  datasetVersionId,
}: {
  datasetId: number;
  datasetVersionId?: number;
}): GetConversationsPendingAction => ({
  type: DatasetAiAssistantActionEnums.getConversationsPending,
  payload: {
    activeDatasetId: datasetId,
    activeDatasetVersionId: datasetVersionId,
    isLoadingConversations: true,
    isPending: true,
    isError: false,
    conversationErrorMessage: undefined,
  },
});

export const getConversationsSuccess = ({
  datasetId,
  datasetVersionId,
  conversations,
  totalCount,
}: {
  datasetId: number;
  datasetVersionId?: number;
  conversations: AIConversation[];
  totalCount: number;
}): GetConversationsSuccessAction => ({
  type: DatasetAiAssistantActionEnums.getConversationsSuccess,
  payload: {
    activeDatasetId: datasetId,
    activeDatasetVersionId: datasetVersionId,
    conversations,
    conversationTotalCount: totalCount,
    isLoadingConversations: false,
    isPending: false,
    isError: false,
    conversationErrorMessage: undefined,
  },
});

export const getConversationsError = ({
  datasetId,
  datasetVersionId,
  errorMessage,
}: {
  datasetId: number;
  datasetVersionId?: number;
  errorMessage: string;
}): GetConversationsErrorAction => ({
  type: DatasetAiAssistantActionEnums.getConversationsError,
  payload: {
    activeDatasetId: datasetId,
    activeDatasetVersionId: datasetVersionId,
    conversations: [],
    conversationTotalCount: 0,
    isLoadingConversations: false,
    isPending: false,
    isError: true,
    conversationErrorMessage: errorMessage,
  },
});

export const getResponsesPending = ({
  conversationId,
}: {
  conversationId: number;
}): GetResponsesPendingAction => ({
  type: DatasetAiAssistantActionEnums.getResponsesPending,
  payload: {
    activeConversationId: conversationId,
    isLoadingResponses: true,
    isPending: true,
    isError: false,
    responseErrorMessage: undefined,
  },
});

export const getResponsesSuccess = ({
  conversationId,
  responses,
  totalCount,
}: {
  conversationId: number;
  responses: AIResponse[];
  totalCount: number;
}): GetResponsesSuccessAction => ({
  type: DatasetAiAssistantActionEnums.getResponsesSuccess,
  payload: {
    activeConversationId: conversationId,
    responses,
    responseTotalCount: totalCount,
    isLoadingResponses: false,
    isPending: false,
    isError: false,
    responseErrorMessage: undefined,
  },
});

export const getResponsesError = ({
  conversationId,
  errorMessage,
}: {
  conversationId: number;
  errorMessage: string;
}): GetResponsesErrorAction => ({
  type: DatasetAiAssistantActionEnums.getResponsesError,
  payload: {
    activeConversationId: conversationId,
    responses: [],
    responseTotalCount: 0,
    isLoadingResponses: false,
    isPending: false,
    isError: true,
    responseErrorMessage: errorMessage,
  },
});

export const generatePending = ({
  datasetVersionId,
  mlExperimentId,
}: {
  datasetVersionId: number;
  mlExperimentId?: number;
}): GeneratePendingAction => ({
  type: DatasetAiAssistantActionEnums.generatePending,
  payload: {
    activeDatasetVersionId: datasetVersionId,
    activeMlExperimentId: mlExperimentId,
    isGenerating: true,
    isPending: true,
    isError: false,
    generationErrorMessage: undefined,
  },
});

export const generateSuccess = ({
  datasetVersionId,
  conversationId,
  response,
}: {
  datasetVersionId: number;
  conversationId: number;
  response: AIResponse;
}): GenerateSuccessAction => ({
  type: DatasetAiAssistantActionEnums.generateSuccess,
  payload: {
    activeDatasetVersionId: datasetVersionId,
    activeConversationId: conversationId,
    activeMlExperimentId: response.mlExperimentId ?? undefined,
    lastGeneratedResponse: response,
    isGenerating: false,
    isPending: false,
    isError: false,
    generationErrorMessage: undefined,
  },
});

export const generateError = ({
  datasetVersionId,
  mlExperimentId,
  errorMessage,
}: {
  datasetVersionId: number;
  mlExperimentId?: number;
  errorMessage: string;
}): GenerateErrorAction => ({
  type: DatasetAiAssistantActionEnums.generateError,
  payload: {
    activeDatasetVersionId: datasetVersionId,
    activeMlExperimentId: mlExperimentId,
    isGenerating: false,
    isPending: false,
    isError: true,
    generationErrorMessage: errorMessage,
  },
});

export const setActiveConversation = (
  conversationId?: number,
): SetActiveConversationAction => ({
  type: DatasetAiAssistantActionEnums.setActiveConversation,
  payload: {
    activeConversationId: conversationId,
    responses: [],
    responseTotalCount: 0,
    responseErrorMessage: undefined,
  },
});

export const clearConversationState = (): ClearConversationStateAction => ({
  type: DatasetAiAssistantActionEnums.clearConversationState,
  payload: {
    activeDatasetId: undefined,
    activeDatasetVersionId: undefined,
    activeConversationId: undefined,
    activeMlExperimentId: undefined,
    activeExperiment: undefined,
    conversations: [],
    responses: [],
    conversationTotalCount: 0,
    responseTotalCount: 0,
    lastGeneratedResponse: undefined,
    isLoadingExperimentContext: false,
    isLoadingConversations: false,
    isLoadingResponses: false,
    isGenerating: false,
    isPending: false,
    isError: false,
    experimentContextErrorMessage: undefined,
    conversationErrorMessage: undefined,
    responseErrorMessage: undefined,
    generationErrorMessage: undefined,
  },
});
