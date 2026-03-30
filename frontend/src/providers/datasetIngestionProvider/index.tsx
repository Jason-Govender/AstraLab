"use client";

import { useContext, useReducer } from "react";
import type { PropsWithChildren } from "react";
import type { UploadRawDatasetFormValues } from "@/types/datasets";
import { uploadRawDataset as uploadRawDatasetRequest } from "@/services/datasetService";
import { getApiErrorMessage } from "@/utils/apiErrors";
import {
  clearFeedback as clearFeedbackAction,
  resetUpload as resetUploadAction,
  uploadError,
  uploadPending,
  uploadSuccess,
} from "./actions";
import {
  DatasetIngestionActionContext,
  DatasetIngestionStateContext,
  INITIAL_STATE,
} from "./context";
import { DatasetIngestionReducer } from "./reducer";

export const DatasetIngestionProvider = ({
  children,
}: PropsWithChildren) => {
  const [state, dispatch] = useReducer(DatasetIngestionReducer, INITIAL_STATE);

  const uploadRawDataset = async (values: UploadRawDatasetFormValues) => {
    dispatch(uploadPending());

    try {
      const uploadResult = await uploadRawDatasetRequest(values);
      dispatch(uploadSuccess(uploadResult));

      return uploadResult;
    } catch (error) {
      const errorMessage = getApiErrorMessage(
        error,
        "Unable to upload this dataset right now.",
      );

      dispatch(uploadError(errorMessage));
      throw error;
    }
  };

  const clearFeedback = () => {
    dispatch(clearFeedbackAction());
  };

  const resetUpload = () => {
    dispatch(resetUploadAction());
  };

  return (
    <DatasetIngestionStateContext.Provider value={state}>
      <DatasetIngestionActionContext.Provider
        value={{
          uploadRawDataset,
          clearFeedback,
          resetUpload,
        }}
      >
        {children}
      </DatasetIngestionActionContext.Provider>
    </DatasetIngestionStateContext.Provider>
  );
};

export const useDatasetIngestionState = () => {
  const context = useContext(DatasetIngestionStateContext);

  if (!context) {
    throw new Error(
      "useDatasetIngestionState must be used within a DatasetIngestionProvider",
    );
  }

  return context;
};

export const useDatasetIngestionActions = () => {
  const context = useContext(DatasetIngestionActionContext);

  if (!context) {
    throw new Error(
      "useDatasetIngestionActions must be used within a DatasetIngestionProvider",
    );
  }

  return context;
};
