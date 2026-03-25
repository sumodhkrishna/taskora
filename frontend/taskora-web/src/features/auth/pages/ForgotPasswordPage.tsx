import { useState } from "react";
import { Link } from "react-router-dom";
import { requestPasswordReset } from "../api/authApi";
import styles from "./ForgotPasswordPage.module.css";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!email.trim()) {
      setErrorMessage("Email is required.");
      setSuccessMessage("");
      return;
    }

    setErrorMessage("");
    setSuccessMessage("");
    setIsSubmitting(true);

    try {
      await requestPasswordReset({
        email: email.trim(),
      });

      setSuccessMessage(
        "If an account exists for that email, a password reset token has been sent."
      );
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to process password reset request.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.backgroundGlowTop} />
      <div className={styles.backgroundGlowBottom} />

      <div className={styles.card}>
        <div className={styles.logo}>Taskora</div>

        <div className={styles.header}>
          <h1 className={styles.title}>Forgot password</h1>
          <p className={styles.subtitle}>
            Enter your email address and we’ll help you reset your password.
          </p>
        </div>

        <form className={styles.form} onSubmit={handleSubmit}>
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

          {errorMessage && (
            <div className={styles.errorMessage}>{errorMessage}</div>
          )}

          {successMessage && (
            <div className={styles.successMessage}>{successMessage}</div>
          )}

          <button
            type="submit"
            className={styles.primaryButton}
            disabled={isSubmitting}
          >
            {isSubmitting ? "Submitting..." : "Send Reset Request"}
          </button>
        </form>

        <div className={styles.footer}>
          <span className={styles.footerText}>Remembered your password?</span>
          <Link to="/" className={styles.link}>
            Sign in
          </Link>
        </div>
      </div>
    </div>
  );
}