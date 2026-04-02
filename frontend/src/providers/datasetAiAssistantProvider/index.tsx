"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import {
  askDatasetQuestion as askDatasetQuestionRequest,
  generateDatasetCleaningRecommendations as generateDatasetCleaningRecommendationsRequest,
  generateDatasetInsights as generateDatasetInsightsRequest,
  generateDatasetSummary as generateDatasetSummaryRequest,
  getDatasetAiConversations as getDatasetAiConversationsRequest,
  getDatasetAiResponses as getDatasetAiResponsesRequest,
} from "@/services/datasetService";
import type {
  AskDatasetAiQuestionRequest,
  GenerateDatasetAiResponseResult,
  GetDatasetAiConversationsRequest,
  GetDatasetAiResponsesRequest,
} from "@/types/datasets";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearConversationState,
  generateError,
  generatePending,
  generateSuccess,
  getConversationsError,
  getConversationsPending,
  getConversationsSuccess,
  getResponsesError,
  getResponsesPending,
  getResponsesSuccess,
  setActiveConversation as setActiveConversationAction,
} from "./actions";
import {
  DatasetAiAssistantActionContext,
  DatasetAiAssistantStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetAiAssistantReducer } from "./reducer";

const DEFAULT_CONVERSATION_PAGE = 1;
const DEFAULT_CONVERSATION_PAGE_SIZE = 20;
const DEFAULT_RESPONSE_PAGE = 1;
const DEFAULT_RESPONSE_PAGE_SIZE = 100;

export const DatasetAiAssistantProvider = ({ children }: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetAiAssistantReducer, INITIAL_STATE);

  const getConversations = async (request: GetDatasetAiConversationsRequest) => {
    dispatch(
      getConversationsPending({
        datasetId: request.datasetId,
        datasetVersionId: request.datasetVersionId,
      }),
    );

    try {
      const result = await getDatasetAiConversationsRequest(request);
      dispatch(
        getConversationsSuccess({
          datasetId: request.datasetId,
          datasetVersionId: request.datasetVersionId,
          conversations: result.items,
          totalCount: result.totalCount,
        }),
      );
    } catch (error) {
      dispatch(
        getConversationsError({
          datasetId: request.datasetId,
          datasetVersionId: request.datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the stored AI conversations right now.",
          ),
        }),
      );
    }
  };

  const getResponses = async (request: GetDatasetAiResponsesRequest) => {
    dispatch(getResponsesPending({ conversationId: request.conversationId }));

    try {
      const result = await getDatasetAiResponsesRequest(request);
      dispatch(
        getResponsesSuccess({
          conversationId: request.conversationId,
          responses: result.items,
          totalCount: result.totalCount,
        }),
      );
    } catch (error) {
      dispatch(
        getResponsesError({
          conversationId: request.conversationId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to load the stored AI responses right now.",
          ),
        }),
      );
    }
  };

  const reloadConversationState = async (
    datasetVersionId: number,
    generatedResult: GenerateDatasetAiResponseResult,
  ) => {
    if (state.activeDatasetId) {
      await getConversations({
        datasetId: state.activeDatasetId,
        datasetVersionId,
        page: DEFAULT_CONVERSATION_PAGE,
        pageSize: DEFAULT_CONVERSATION_PAGE_SIZE,
      });
    }

    await getResponses({
      conversationId: generatedResult.conversationId,
      page: DEFAULT_RESPONSE_PAGE,
      pageSize: DEFAULT_RESPONSE_PAGE_SIZE,
      isChronological: true,
    });
  };

  const runGeneration = async (
    datasetVersionId: number,
    request: () => Promise<GenerateDatasetAiResponseResult>,
  ) => {
    dispatch(generatePending({ datasetVersionId }));

    try {
      const result = await request();
      dispatch(
        generateSuccess({
          datasetVersionId,
          conversationId: result.conversationId,
          response: result.response,
        }),
      );
      await reloadConversationState(datasetVersionId, result);
    } catch (error) {
      dispatch(
        generateError({
          datasetVersionId,
          errorMessage: getApiErrorMessage(
            error,
            "Unable to generate the dataset AI response right now.",
          ),
        }),
      );
    }
  };

  const generateSummary = async (datasetVersionId: number) => {
    await runGeneration(datasetVersionId, () =>
      generateDatasetSummaryRequest(datasetVersionId),
    );
  };

  const generateInsights = async (datasetVersionId: number) => {
    await runGeneration(datasetVersionId, () =>
      generateDatasetInsightsRequest(datasetVersionId),
    );
  };

  const generateCleaningRecommendations = async (datasetVersionId: number) => {
    await runGeneration(datasetVersionId, () =>
      generateDatasetCleaningRecommendationsRequest(datasetVersionId),
    );
  };

  const askQuestion = async (request: AskDatasetAiQuestionRequest) => {
    await runGeneration(request.datasetVersionId, () =>
      askDatasetQuestionRequest({
        datasetVersionId: request.datasetVersionId,
        question: request.question,
        conversationId: request.conversationId ?? state.activeConversationId,
      }),
    );
  };

  const setActiveConversation = (conversationId?: number) => {
    dispatch(setActiveConversationAction(conversationId));
  };

  const clearAssistantState = () => {
    dispatch(clearConversationState());
  };

  return (
    <DatasetAiAssistantStateContext.Provider value={state}>
      <DatasetAiAssistantActionContext.Provider
        value={{
          generateSummary,
          generateInsights,
          generateCleaningRecommendations,
          askQuestion,
          getConversations,
          getResponses,
          setActiveConversation,
          clearConversationState: clearAssistantState,
        }}
      >
        {children}
      </DatasetAiAssistantActionContext.Provider>
    </DatasetAiAssistantStateContext.Provider>
  );
};

export const useDatasetAiAssistantState = () => {
  const context = useContext(DatasetAiAssistantStateContext);

  if (!context) {
    throw new Error(
      "useDatasetAiAssistantState must be used within a DatasetAiAssistantProvider",
    );
  }

  return context;
};

export const useDatasetAiAssistantActions = () => {
  const context = useContext(DatasetAiAssistantActionContext);

  if (!context) {
    throw new Error(
      "useDatasetAiAssistantActions must be used within a DatasetAiAssistantProvider",
    );
  }

  return context;
};
