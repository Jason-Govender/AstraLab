"use client";

import { useEffect, useEffectEvent, useRef } from "react";
import { LOGIN_ROUTE } from "@/constants/auth";
import { useAuthActions } from "@/providers/authProvider";
import { setUnauthorizedHandler } from "@/utils/axiosInstance";

export function AuthBootstrap() {
  const { initializeAuth, logout } = useAuthActions();
  const hasInitialized = useRef(false);

  const handleUnauthorized = useEffectEvent(() => {
    logout({ redirectTo: LOGIN_ROUTE });
  });

  useEffect(() => {
    if (hasInitialized.current) {
      return;
    }

    hasInitialized.current = true;
    void initializeAuth();
  }, [initializeAuth]);

  useEffect(() => {
    setUnauthorizedHandler(() => {
      handleUnauthorized();
    });

    return () => {
      setUnauthorizedHandler(null);
    };
  }, []);

  return null;
}
