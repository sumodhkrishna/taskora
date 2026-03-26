import { useState } from "react";
import { AxiosError } from "axios";
import { useNavigate, Link } from "react-router-dom";
import { login } from "../api/authApi";
import { saveAuthSession } from "../services/authStorage";
import styles from "./LoginPage.module.css";

export function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const navigate = useNavigate();

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (!email.trim() || !password.trim()) {
            setErrorMessage("Email and password are required.");
            return;
        }

        setErrorMessage("");
        setIsSubmitting(true);

        try {
            const response = await login({
                email,
                password,
            });

            saveAuthSession(
                response.accessToken,
                response.refreshToken,
                response.userId,
                response.name,
                response.email
            );

            navigate("/home");
        } catch (error: unknown) {
            const axiosError = error instanceof AxiosError ? error : null;
            const status = axiosError?.response?.status;
            const detail =
                typeof axiosError?.response?.data === "object" &&
                axiosError.response?.data &&
                "detail" in axiosError.response.data
                    ? String(axiosError.response.data.detail)
                    : "";

            if (status === 403) {
                setErrorMessage(detail || "Please verify your email before signing in.");
            } else {
                setErrorMessage("Invalid email or password.");
            }

            console.error(error);
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div className={styles.page}>
            <div className={styles.backgroundGlowTop} />
            <div className={styles.backgroundGlowBottom} />

            <div className={styles.card}>
                <div className={styles.logoRow}>
                    <img src="/favicon.svg" alt="" className={styles.logoIcon} />
                    <div className={styles.logo}>Taskora</div>
                </div>

                <div className={styles.header}>
                    <h1 className={styles.title}>Welcome back</h1>
                    <p className={styles.subtitle}>
                        Sign in to continue managing your tasks.
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
                        <label htmlFor="password" className={styles.label}>
                            Password
                        </label>
                        <input
                            id="password"
                            type="password"
                            placeholder="Enter your password"
                            className={styles.input}
                            value={password}
                            onChange={(event) => setPassword(event.target.value)}
                            autoComplete="current-password"
                        />
                    </div>

                    {errorMessage && (
                        <div className={styles.errorMessage}>{errorMessage}</div>
                    )}

                    <button
                        type="submit"
                        className={styles.signInButton}
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? "Signing In..." : "Sign In"}
                    </button>
                </form>

                <div className={styles.footer}>
                    <span className={styles.footerText}>Don't have an account?</span>
                    <Link to="/register" className={styles.link}>
                        Create one
                    </Link>
                </div>
                <div className={styles.secondaryFooter}>
                    <Link to="/forgot-password" className={styles.link}>
                        Forgot password?
                    </Link>
                </div>
                <div className={styles.secondaryFooter}>
                    <Link to="/verify-email" className={styles.link}>
                        Verify or resend email
                    </Link>
                </div>
            </div>
        </div>
    );
}
