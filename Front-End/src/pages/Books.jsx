import { useEffect, useState } from 'react';
import { getBooks } from '../services/bookService';

export default function Books() {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getBooks()
      .then(({ data }) => setBooks(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <p className="muted">Loading books...</p>;
  }

  if (error) {
    return <p className="error">Failed to load books: {error}</p>;
  }

  return (
    <div>
      <h1>Books</h1>
      <table className="data-table">
        <thead>
          <tr>
            <th>Title</th>
            <th>Author</th>
            <th>ISBN</th>
            <th>Available</th>
          </tr>
        </thead>
        <tbody>
          {books.map((book) => (
            <tr key={book.id}>
              <td>{book.title}</td>
              <td>{book.author}</td>
              <td>{book.isbn}</td>
              <td>{book.availableCopies}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {books.length === 0 && <p className="muted">No books found.</p>}
    </div>
  );
}
