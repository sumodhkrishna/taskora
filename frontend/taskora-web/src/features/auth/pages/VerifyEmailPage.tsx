import { useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import { resendVerificationEmail, verifyEmail } from "../api/authApi";
import styles from "./VerifyEmailPage.module.css";

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const initialEmail = useMemo(() => searchParams.get("email") ?? "", [searchParams]);
  const initialToken = useMemo(() => searchParams.get("token") ?? "", [searchParams]);

  const [email, setEmail] = useState(initialEmail);
  const [token, setToken] = useState(initialToken);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [resendMessage, setResendMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isResending, setIsResending] = useState(false);

  async function handleVerify(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!email.trim() || !token.trim()) {
      setErrorMessage("Email and verification token are required.");
      setSuccessMessage("");
      return;
    }

    setErrorMessage("");
    setSuccessMessage("");
    setIsSubmitting(true);

    try {
      const response = await verifyEmail({
        email: email.trim(),
        token: token.trim(),
      });

      setSuccessMessage(response.message);
    } catch (error: unknown) {
      const axiosError = error instanceof AxiosError ? error : null;
      const detail =
        typeof axiosError?.response?.data === "object" &&
        axiosError.response?.data &&
        "message" in axiosError.response.data
          ? String(axiosError.response.data.message)
          : "Unable to verify your email.";
      setErrorMessage(detail);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleResend() {
    if (!email.trim()) {
      setErrorMessage("Email is required to resend verification.");
      setResendMessage("");
      return;
    }

    setErrorMessage("");
    setResendMessage("");
    setIsResending(true);

    try {
      const response = await resendVerificationEmail({ email: email.trim() });
      setResendMessage(response.message);
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to resend verification email.");
    } finally {
      setIsResending(false);
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.backgroundGlowTop} />
      <div className={styles.backgroundGlowBottom} />

      <div className={styles.card}>
        <div className={styles.logo}>Taskora</div>

        <div className={styles.header}>
          <h1 className={styles.title}>Verify your email</h1>
          <p className={styles.subtitle}>
            Enter the verification email details below or request a new verification email.
          </p>
        </div>

        <form className={styles.form} onSubmit={handleVerify}>
          <div className={styles.field}>
            <label htmlFor="email" className={styles.label}>
              Email
            </label>
            <input
              id="email"
              type="email"
              placeholder="Enter your email"
              className={styles.input}
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="email"
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="token" className={styles.label}>
              Verification Token
            </label>
            <input
              id="token"
              type="text"
              placeholder="Paste your verification token"
              className={styles.input}
              value={token}
              onChange={(event) => setToken(event.target.value)}
            />
          </div>

          {errorMessage && <div className={styles.errorMessage}>{errorMessage}</div>}
          {successMessage && <div className={styles.successMessage}>{successMessage}</div>}
          {resendMessage && <div className={styles.successMessage}>{resendMessage}</div>}

          <button
            type="submit"
            className={styles.primaryButton}
            disabled={isSubmitting}
          >
            {isSubmitting ? "Verifying..." : "Verify Email"}
          </button>

          <button
            type="button"
            className={styles.secondaryButton}
            disabled={isResending}
            onClick={() => void handleResend()}
          >
            {isResending ? "Sending..." : "Resend Verification Email"}
          </button>
        </form>

        <div className={styles.footer}>
          <span className={styles.footerText}>Already verified?</span>
          <Link to="/" className={styles.link}>
            Back to sign in
          </Link>
        </div>
      </div>
    </div>
  );
}
