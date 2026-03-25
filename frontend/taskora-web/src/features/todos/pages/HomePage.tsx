import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { logout } from "../../auth/api/authApi";
import {
  clearAuthSession,
  getRefreshToken,
  getStoredUserName,
} from "../../auth/services/authStorage";
import {
  completeTodo,
  createTodo,
  deleteTodo,
  getTodos,
  reopenTodo,
  updateTodo,
} from "../api/todoApi";
import type { CreateTodoRequest, TodoDto, TodoPriority, UpdateTodoRequest } from "../types/todo";
import styles from "./HomePage.module.css";

function toDateInputValue(value: string | null): string {
  if (!value) {
    return "";
  }

  const normalized = value.slice(0, 10);
  if (!/^\d{4}-\d{2}-\d{2}$/.test(normalized)) {
    return "";
  }

  return normalized;
}

function toUtcDateString(value: string): string | null {
  if (!value) {
    return null;
  }

  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatDateLabel(value: string | null): string {
  if (!value) {
    return "";
  }

  const normalized = value.slice(0, 10);
  const [year, month, day] = normalized.split("-").map(Number);

  if (!year || !month || !day) {
    return "";
  }

  return new Intl.DateTimeFormat(undefined, {
    month: "numeric",
    day: "numeric",
    year: "numeric",
    timeZone: "UTC",
  }).format(new Date(Date.UTC(year, month - 1, day)));
}

function getPriorityLabel(priority: number | string): string {
  const normalized = Number(priority);

  if (normalized === 1) return "Low";
  if (normalized === 2) return "Medium";
  if (normalized === 3) return "High";

  return "Unknown";
}

export function HomePage() {
  const navigate = useNavigate();

  const [todos, setTodos] = useState<TodoDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [editingTodoId, setEditingTodoId] = useState<number | string | null>(null);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<TodoPriority>(2);
  const [toBeCompletedByDate, setToBeCompletedByDate] = useState("");

  const userName = getStoredUserName();

  useEffect(() => {
    void loadTodos();
  }, []);

  async function loadTodos() {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const data = await getTodos();
      setTodos(data);
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to load your tasks.");
    } finally {
      setIsLoading(false);
    }
  }

  function resetForm() {
    setEditingTodoId(null);
    setTitle("");
    setDescription("");
    setPriority(2);
    setToBeCompletedByDate("");
    setErrorMessage("");
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!title.trim()) {
      setErrorMessage("Title is required.");
      return;
    }

    if (title.trim().length > 200) {
      setErrorMessage("Title must be 200 characters or less.");
      return;
    }

    if (description.trim().length > 2000) {
      setErrorMessage("Description must be 2000 characters or less.");
      return;
    }

    setIsSaving(true);
    setErrorMessage("");

    try {
      const payload: CreateTodoRequest | UpdateTodoRequest = {
        title: title.trim(),
        description: description.trim() ? description.trim() : null,
        priority,
        toBeCompletedByDateUtc: toUtcDateString(toBeCompletedByDate),
      };

      if (editingTodoId !== null) {
        const updated = await updateTodo(editingTodoId, payload);
        setTodos((current) =>
          current.map((todo) => (todo.id === editingTodoId ? updated : todo))
        );
      } else {
        const created = await createTodo(payload);
        setTodos((current) => [created, ...current]);
      }

      resetForm();
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to save the task.");
    } finally {
      setIsSaving(false);
    }
  }

  function handleEdit(todo: TodoDto) {
    setEditingTodoId(todo.id);
    setTitle(todo.title);
    setDescription(todo.description ?? "");
    setPriority(Number(todo.priority) as TodoPriority);
    setToBeCompletedByDate(toDateInputValue(todo.toBeCompletedByDateUtc));
    setErrorMessage("");
  }

  async function handleToggleComplete(todo: TodoDto) {
    try {
      const updated = todo.isCompleted
        ? await reopenTodo(todo.id)
        : await completeTodo(todo.id);

      setTodos((current) =>
        current.map((item) => (item.id === todo.id ? updated : item))
      );
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to update task status.");
    }
  }

  async function handleDelete(id: number | string) {
    try {
      await deleteTodo(id);
      setTodos((current) => current.filter((todo) => todo.id !== id));

      if (editingTodoId === id) {
        resetForm();
      }
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to delete the task.");
    }
  }

  async function handleLogout() {
    const refreshToken = getRefreshToken();

    try {
      if (refreshToken) {
        await logout({ refreshToken });
      }
    } catch (error) {
      console.error(error);
    } finally {
      clearAuthSession();
      navigate("/");
    }
  }

  const totalCount = todos.length;
  const completedCount = todos.filter((todo) => todo.isCompleted).length;
  const pendingCount = totalCount - completedCount;

  const sortedTodos = useMemo(() => {
    return [...todos].sort((a, b) => {
      if (a.isCompleted !== b.isCompleted) {
        return a.isCompleted ? 1 : -1;
      }

      const priorityDiff = Number(b.priority) - Number(a.priority);
      if (priorityDiff !== 0) {
        return priorityDiff;
      }

      const aDate = a.toBeCompletedByDateUtc ? new Date(a.toBeCompletedByDateUtc).getTime() : Infinity;
      const bDate = b.toBeCompletedByDateUtc ? new Date(b.toBeCompletedByDateUtc).getTime() : Infinity;

      return aDate - bDate;
    });
  }, [todos]);

  return (
    <div className={styles.page}>
      <div className={styles.backgroundGlowTop} />
      <div className={styles.backgroundGlowBottom} />

      <div className={styles.shell}>
        <header className={styles.header}>
          <div>
            <div className={styles.brand}>Taskora</div>
            <h1 className={styles.pageTitle}>Your tasks</h1>
            <p className={styles.pageSubtitle}>
              Create, track, complete, and organize your work in one place.
            </p>
          </div>

          <div className={styles.headerActions}>
            <div className={styles.userBadge}>
              {userName ? `Hi, ${userName}` : "Welcome"}
            </div>

            <button type="button" className={styles.logoutButton} onClick={handleLogout}>
              Log Out
            </button>
          </div>
        </header>

        <section className={styles.summaryGrid}>
          <div className={styles.summaryCard}>
            <span className={styles.summaryLabel}>Total Tasks</span>
            <strong className={styles.summaryValue}>{totalCount}</strong>
          </div>

          <div className={styles.summaryCard}>
            <span className={styles.summaryLabel}>Pending</span>
            <strong className={styles.summaryValue}>{pendingCount}</strong>
          </div>

          <div className={styles.summaryCard}>
            <span className={styles.summaryLabel}>Completed</span>
            <strong className={styles.summaryValue}>{completedCount}</strong>
          </div>
        </section>

        <section className={styles.contentGrid}>
          <div className={styles.panel}>
            <div className={styles.panelHeader}>
              <h2 className={styles.panelTitle}>
                {editingTodoId !== null ? "Edit task" : "Create a new task"}
              </h2>
              {editingTodoId !== null && (
                <button type="button" className={styles.textButton} onClick={resetForm}>
                  Cancel edit
                </button>
              )}
            </div>

            <form className={styles.form} onSubmit={handleSubmit}>
              <div className={styles.field}>
                <label htmlFor="title" className={styles.label}>
                  Title
                </label>
                <input
                  id="title"
                  className={styles.input}
                  type="text"
                  placeholder="Enter task title"
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  maxLength={200}
                />
              </div>

              <div className={styles.field}>
                <label htmlFor="description" className={styles.label}>
                  Description
                </label>
                <textarea
                  id="description"
                  className={styles.textarea}
                  placeholder="Add more detail"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  maxLength={2000}
                />
              </div>

              <div className={styles.row}>
                <div className={styles.field}>
                  <label htmlFor="priority" className={styles.label}>
                    Priority
                  </label>
                  <select
                    id="priority"
                    className={styles.select}
                    value={priority}
                    onChange={(event) => setPriority(Number(event.target.value) as TodoPriority)}
                  >
                    <option value={1}>Low</option>
                    <option value={2}>Medium</option>
                    <option value={3}>High</option>
                  </select>
                </div>

                <div className={styles.field}>
                  <label htmlFor="toBeCompletedByDate" className={styles.label}>
                    Due Date
                  </label>
                  <input
                    id="toBeCompletedByDate"
                    className={styles.input}
                    type="date"
                    value={toBeCompletedByDate}
                    onChange={(event) => setToBeCompletedByDate(event.target.value)}
                  />
                </div>
              </div>

              {errorMessage && <div className={styles.errorMessage}>{errorMessage}</div>}

              <button type="submit" className={styles.primaryButton} disabled={isSaving}>
                {isSaving
                  ? editingTodoId !== null
                    ? "Saving..."
                    : "Creating..."
                  : editingTodoId !== null
                    ? "Save Changes"
                    : "Create Task"}
              </button>
            </form>
          </div>

          <div className={styles.panel}>
            <div className={styles.panelHeader}>
              <h2 className={styles.panelTitle}>All tasks</h2>
              <button type="button" className={styles.textButton} onClick={() => void loadTodos()}>
                Refresh
              </button>
            </div>

            {isLoading ? (
              <div className={styles.emptyState}>Loading tasks...</div>
            ) : sortedTodos.length === 0 ? (
              <div className={styles.emptyState}>
                No tasks yet. Create your first one.
              </div>
            ) : (
              <div className={styles.todoList}>
                {sortedTodos.map((todo) => (
                  <article
                    key={String(todo.id)}
                    className={`${styles.todoCard} ${todo.isCompleted ? styles.todoCardCompleted : ""}`}
                  >
                    <div className={styles.todoTop}>
                      <div>
                        <h3 className={styles.todoTitle}>{todo.title}</h3>
                        <div className={styles.metaRow}>
                          <span className={styles.priorityBadge}>
                            {getPriorityLabel(todo.priority)}
                          </span>
                          <span className={styles.metaText}>
                            {todo.toBeCompletedByDateUtc
                              ? `Due ${formatDateLabel(todo.toBeCompletedByDateUtc)}`
                              : "No due date"}
                          </span>
                        </div>
                      </div>

                      <button
                        type="button"
                        className={styles.statusButton}
                        onClick={() => void handleToggleComplete(todo)}
                      >
                        {todo.isCompleted ? "Reopen" : "Complete"}
                      </button>
                    </div>

                    {todo.description && (
                      <p className={styles.todoDescription}>{todo.description}</p>
                    )}

                    <div className={styles.todoFooter}>
                      <span className={styles.metaText}>
                        {todo.isCompleted && todo.completedAtUtc
                          ? `Completed ${new Date(todo.completedAtUtc).toLocaleDateString()}`
                          : `Created ${new Date(todo.createdAtUtc).toLocaleDateString()}`}
                      </span>

                      <div className={styles.todoActions}>
                        <button
                          type="button"
                          className={styles.secondaryButton}
                          onClick={() => handleEdit(todo)}
                        >
                          Edit
                        </button>

                        <button
                          type="button"
                          className={styles.dangerButton}
                          onClick={() => void handleDelete(todo.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </div>
        </section>
      </div>
    </div>
  );
}
