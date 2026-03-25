import { useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { confirmPasswordReset } from "../api/authApi";
import styles from "./ResetPasswordPage.module.css";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();

  const initialEmail = useMemo(() => searchParams.get("email") ?? "", [searchParams]);
  const initialToken = useMemo(() => searchParams.get("token") ?? "", [searchParams]);

  const [email, setEmail] = useState(initialEmail);
  const [token, setToken] = useState(initialToken);
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!email.trim() || !token.trim() || !newPassword.trim() || !confirmPassword.trim()) {
      setErrorMessage("All fields are required.");
      setSuccessMessage("");
      return;
    }

    if (newPassword.length < 6 || newPassword.length > 100) {
      setErrorMessage("New password must be between 6 and 100 characters.");
      setSuccessMessage("");
      return;
    }

    if (newPassword !== confirmPassword) {
      setErrorMessage("Passwords do not match.");
      setSuccessMessage("");
      return;
    }

    setErrorMessage("");
    setSuccessMessage("");
    setIsSubmitting(true);

    try {
      await confirmPasswordReset({
        email: email.trim(),
        token: token.trim(),
        newPassword,
      });

      setSuccessMessage("Your password has been reset successfully.");
      setNewPassword("");
      setConfirmPassword("");
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to reset password. Check the token and email.");
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
          <h1 className={styles.title}>Set a new password</h1>
          <p className={styles.subtitle}>
            Enter your reset token and choose a new password to regain access.
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

          <div className={styles.field}>
            <label htmlFor="token" className={styles.label}>
              Reset Token
            </label>
            <input
              id="token"
              type="text"
              placeholder="Paste your reset token"
              className={styles.input}
              value={token}
              onChange={(event) => setToken(event.target.value)}
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="newPassword" className={styles.label}>
              New Password
            </label>
            <input
              id="newPassword"
              type="password"
              placeholder="Enter new password"
              className={styles.input}
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              autoComplete="new-password"
              minLength={6}
              maxLength={100}
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="confirmPassword" className={styles.label}>
              Confirm New Password
            </label>
            <input
              id="confirmPassword"
              type="password"
              placeholder="Confirm new password"
              className={styles.input}
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              autoComplete="new-password"
              minLength={6}
              maxLength={100}
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
            {isSubmitting ? "Resetting..." : "Reset Password"}
          </button>
        </form>

        <div className={styles.footer}>
          <span className={styles.footerText}>Want to try again from login?</span>
          <Link to="/" className={styles.link}>
            Back to sign in
          </Link>
        </div>
      </div>
    </div>
  );
}