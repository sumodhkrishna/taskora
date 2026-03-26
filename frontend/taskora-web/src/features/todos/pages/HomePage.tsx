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
import type {
  CreateTodoRequest,
  TodoDto,
  TodoPriority,
  TodoStatusFilter,
  UpdateTodoRequest,
} from "../types/todo";
import styles from "./HomePage.module.css";

function toDateInputValue(value: string | null): string {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

function toUtcDateString(value: string): string | null {
  if (!value) return null;
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function getPriorityLabel(priority: number | string): string {
  const normalized = Number(priority);
  if (normalized === 1) return "Low";
  if (normalized === 2) return "Medium";
  if (normalized === 3) return "High";
  return "Unknown";
}

function mapStatusFilterToApi(status: TodoStatusFilter): string | undefined {
  if (status === "all") return undefined;
  if (status === "pending") return "Pending";
  if (status === "completed") return "Completed";
  return undefined;
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

  const [statusFilter, setStatusFilter] = useState<TodoStatusFilter>("all");
  const [priorityFilter, setPriorityFilter] = useState<string>("all");
  const [searchFilter, setSearchFilter] = useState("");
  const [dueBeforeFilter, setDueBeforeFilter] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(6);

  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const userName = getStoredUserName();

  useEffect(() => {
    void loadTodos();
  }, [statusFilter, priorityFilter, searchFilter, dueBeforeFilter, page, pageSize]);

  async function loadTodos() {
    setIsLoading(true);
    setErrorMessage("");

    try {
      const result = await getTodos({
        status: mapStatusFilterToApi(statusFilter),
        priority: priorityFilter === "all" ? undefined : Number(priorityFilter),
        search: searchFilter.trim() || undefined,
        dueBefore: dueBeforeFilter
          ? new Date(`${dueBeforeFilter}T23:59:59.999Z`).toISOString()
          : undefined,
        page,
        pageSize,
      });

      setTodos(result.items ?? []);
      setTotalCount(Number(result.totalCount ?? 0));
      setTotalPages(Math.max(1, Number(result.totalPages ?? 1)));
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

  function resetFilters() {
    setStatusFilter("all");
    setPriorityFilter("all");
    setSearchFilter("");
    setDueBeforeFilter("");
    setPage(1);
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
        await updateTodo(editingTodoId, payload);
      } else {
        await createTodo(payload);
      }

      resetForm();
      await loadTodos();
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
      if (todo.isCompleted) {
        await reopenTodo(todo.id);
      } else {
        await completeTodo(todo.id);
      }

      await loadTodos();
    } catch (error) {
      console.error(error);
      setErrorMessage("Unable to update task status.");
    }
  }

  async function handleDelete(id: number | string) {
    try {
      await deleteTodo(id);

      if (editingTodoId === id) {
        resetForm();
      }

      const isLastItemOnPage = todos.length === 1 && page > 1;
      if (isLastItemOnPage) {
        setPage((current) => current - 1);
      } else {
        await loadTodos();
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

  const completedOnPage = useMemo(
    () => todos.filter((todo) => todo.isCompleted).length,
    [todos]
  );

  const pendingOnPage = todos.length - completedOnPage;

  const canGoPrevious = page > 1;
  const canGoNext = page < totalPages;

  return (
    <div className={styles.page}>
      <div className={styles.backgroundGlowTop} />
      <div className={styles.backgroundGlowBottom} />

      <div className={styles.shell}>
        <header className={styles.header}>
          <div>
            <div className={styles.brandRow}>
              <img src="/favicon.svg" alt="" className={styles.brandIcon} />
              <div className={styles.brand}>Taskora</div>
            </div>
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
            <span className={styles.summaryLabel}>Filtered Total</span>
            <strong className={styles.summaryValue}>{totalCount}</strong>
          </div>

          <div className={styles.summaryCard}>
            <span className={styles.summaryLabel}>Pending On Page</span>
            <strong className={styles.summaryValue}>{pendingOnPage}</strong>
          </div>

          <div className={styles.summaryCard}>
            <span className={styles.summaryLabel}>Completed On Page</span>
            <strong className={styles.summaryValue}>{completedOnPage}</strong>
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
              <button type="button" className={styles.textButton} onClick={resetFilters}>
                Reset filters
              </button>
            </div>

            <div className={styles.filterBar}>
              <input
                type="text"
                className={styles.filterInput}
                placeholder="Search tasks"
                value={searchFilter}
                onChange={(event) => {
                  setSearchFilter(event.target.value);
                  setPage(1);
                }}
              />

              <select
                className={styles.filterSelect}
                value={statusFilter}
                onChange={(event) => {
                  setStatusFilter(event.target.value as TodoStatusFilter);
                  setPage(1);
                }}
              >
                <option value="all">All Statuses</option>
                <option value="pending">Pending</option>
                <option value="completed">Completed</option>
              </select>

              <select
                className={styles.filterSelect}
                value={priorityFilter}
                onChange={(event) => {
                  setPriorityFilter(event.target.value);
                  setPage(1);
                }}
              >
                <option value="all">All Priorities</option>
                <option value="1">Low</option>
                <option value="2">Medium</option>
                <option value="3">High</option>
              </select>

              <input
                type="date"
                className={styles.filterInput}
                value={dueBeforeFilter}
                onChange={(event) => {
                  setDueBeforeFilter(event.target.value);
                  setPage(1);
                }}
              />

              <select
                className={styles.filterSelect}
                value={pageSize}
                onChange={(event) => {
                  setPageSize(Number(event.target.value));
                  setPage(1);
                }}
              >
                <option value={6}>6 / page</option>
                <option value={9}>9 / page</option>
                <option value={12}>12 / page</option>
              </select>
            </div>

            {isLoading ? (
              <div className={styles.emptyState}>Loading tasks...</div>
            ) : todos.length === 0 ? (
              <div className={styles.emptyState}>
                No tasks matched the current filters.
              </div>
            ) : (
              <>
                <div className={styles.todoList}>
                  {todos.map((todo) => (
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
                                ? `Due ${new Date(todo.toBeCompletedByDateUtc).toLocaleDateString()}`
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

                <div className={styles.paginationBar}>
                  <div className={styles.paginationInfo}>
                    Page {page} of {totalPages}
                  </div>

                  <div className={styles.paginationActions}>
                    <button
                      type="button"
                      className={styles.secondaryButton}
                      disabled={!canGoPrevious}
                      onClick={() => setPage((current) => current - 1)}
                    >
                      Previous
                    </button>

                    <button
                      type="button"
                      className={styles.secondaryButton}
                      disabled={!canGoNext}
                      onClick={() => setPage((current) => current + 1)}
                    >
                      Next
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        </section>
      </div>
    </div>
  );
}
