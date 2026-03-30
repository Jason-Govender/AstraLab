"use client";

import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  loadingState: css`
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background:
      radial-gradient(circle at top left, rgba(47, 140, 255, 0.16), transparent 24%),
      linear-gradient(180deg, #09111f 0%, #070d19 100%);
  `,

  page: css`
    min-height: 100vh;
    padding: 14px;
    background:
      radial-gradient(circle at top left, rgba(47, 140, 255, 0.12), transparent 22%),
      radial-gradient(circle at bottom right, rgba(255, 122, 26, 0.08), transparent 20%),
      linear-gradient(180deg, #09111f 0%, #070d19 100%);
  `,

  frame: css`
    min-height: calc(100vh - 28px);
    display: grid;
    grid-template-columns: 240px minmax(0, 1fr);
    overflow: hidden;
    border: 1px solid rgba(33, 49, 77, 0.95);
    border-radius: 28px;
    background: rgba(8, 15, 28, 0.96);
    box-shadow: 0 32px 96px rgba(0, 0, 0, 0.34);

    @media (max-width: 1024px) {
      grid-template-columns: minmax(0, 1fr);
    }
  `,

  sidebar: css`
    display: flex;
    flex-direction: column;
    padding: 28px 16px 24px;
    border-right: 1px solid rgba(33, 49, 77, 0.92);
    background: rgba(17, 26, 45, 0.92);

    @media (max-width: 1024px) {
      display: none;
    }
  `,

  brand: css`
    margin: 0;
    color: ${token.colorText} !important;
    font-size: 18px !important;
    font-weight: 700 !important;
    letter-spacing: -0.03em;
  `,

  mobileBrand: css`
    display: none;
    margin: 0 !important;
    color: ${token.colorText} !important;

    @media (max-width: 1024px) {
      display: block;
    }
  `,

  navigationSection: css`
    margin-top: 48px;
  `,

  sectionLabel: css`
    display: block;
    margin-bottom: 14px;
    padding: 0 10px;
    color: ${token.colorTextSecondary};
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  `,

  navigationList: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,

  navItem: css`
    display: flex;
    align-items: center;
    gap: 12px;
    min-height: 44px;
    padding: 0 14px;
    border: 1px solid transparent;
    border-radius: 14px;
    color: ${token.colorTextSecondary};
    font-size: 15px;
    font-weight: 500;
    text-decoration: none;
    transition:
      background 0.2s ease,
      border-color 0.2s ease,
      color 0.2s ease;

    &:hover {
      color: ${token.colorText};
      border-color: rgba(42, 58, 93, 0.7);
      background: rgba(21, 33, 57, 0.8);
    }
  `,

  navItemActive: css`
    color: ${token.colorText};
    border-color: rgba(42, 58, 93, 0.92);
    background: rgba(34, 49, 79, 0.95);
  `,

  navItemDisabled: css`
    cursor: not-allowed;
    opacity: 0.58;

    &:hover {
      color: ${token.colorTextSecondary};
      border-color: transparent;
      background: transparent;
    }
  `,

  navIcon: css`
    font-size: 17px;
  `,

  contentColumn: css`
    min-width: 0;
    display: flex;
    flex-direction: column;
  `,

  header: css`
    display: flex;
    align-items: center;
    gap: 18px;
    padding: 14px 22px;
    border-bottom: 1px solid rgba(33, 49, 77, 0.9);
    background: rgba(17, 25, 44, 0.88);

    @media (max-width: 768px) {
      flex-wrap: wrap;
      padding: 14px 16px;
    }
  `,

  mobileMenuButton: css`
    display: none;

    @media (max-width: 1024px) {
      &.ant-btn {
        display: inline-flex;
      }
    }
  `,

  search: css`
    flex: 1;
    min-width: 260px;
    max-width: 520px;

    &.ant-input-affix-wrapper {
      border-radius: 999px;
      background: rgba(26, 37, 64, 0.92);
    }

    @media (max-width: 768px) {
      order: 3;
      width: 100%;
      max-width: 100%;
      min-width: 100%;
    }
  `,

  headerUtilities: css`
    display: flex;
    align-items: center;
    gap: 12px;
    margin-left: auto;
  `,

  utilityButton: css`
    &.ant-btn {
      min-width: 118px;
      border-radius: 999px;
    }
  `,

  profileButton: css`
    &.ant-btn {
      height: auto;
      padding: 4px;
      border: none;
      background: transparent;
      box-shadow: none;
    }
  `,

  avatar: css`
    background: ${token.colorPrimary};
    color: #fff;
    font-weight: 700;
  `,

  dropdownMeta: css`
    display: flex;
    flex-direction: column;
    gap: 2px;
  `,

  dropdownTitle: css`
    color: ${token.colorText};
    font-weight: 600;
  `,

  dropdownText: css`
    color: ${token.colorTextSecondary};
    font-size: 12px;
  `,

  contentViewport: css`
    flex: 1;
    min-width: 0;
    overflow: auto;
  `,

  contentInner: css`
    width: 100%;
    max-width: 1240px;
    margin: 0 auto;
    padding: 24px 24px 40px;

    @media (max-width: 768px) {
      padding: 20px 16px 28px;
    }
  `,

  mobileDrawerContent: css`
    display: flex;
    flex-direction: column;
    height: 100%;
    padding: 12px 0;
    background: transparent;
  `,

  pageHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 20px;
    margin-bottom: 26px;

    @media (max-width: 768px) {
      flex-direction: column;
      margin-bottom: 22px;
    }
  `,

  pageHeaderTitle: css`
    margin: 0 0 6px !important;
    color: ${token.colorText} !important;
    font-size: clamp(34px, 3vw, 44px) !important;
    letter-spacing: -0.04em !important;
  `,

  pageHeaderDescription: css`
    max-width: 760px;
    margin: 0 !important;
    color: ${token.colorTextSecondary} !important;
    font-size: 17px;
  `,
}));
