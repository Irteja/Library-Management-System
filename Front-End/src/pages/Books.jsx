import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { getBooks, createBook, updateBook } from '../services/bookService';
import { getBranchesCursor } from '../services/branchService';

const loadAllBranches = async () => {
  let allBranches = [];
  let currentCursor = null;
  let hasNext = true;
  
  while (hasNext) {
    const res = await getBranchesCursor(currentCursor, 100);
    allBranches = [...allBranches, ...res.data.items];
    currentCursor = res.data.nextCursor;
    hasNext = res.data.hasNextPage;
  }
  return allBranches;
};

const emptyForm = {
  isbn: '',
  title: '',
  author: '',
  publisher: '',
  publicationYear: new Date().getFullYear(),
  category: '',
  totalCopies: 1,
  availableCopies: 1,
  branchId: '',
};

export default function Books() {
  const { user } = useAuth();
  const isStaff = user?.role === 'Admin' || user?.role === 'Librarian';

  const [books, setBooks] = useState([]);
  const [branches, setBranches] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const itemsPerPage = 10;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showForm, setShowForm] = useState(false);
  const [editingBookId, setEditingBookId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const loadData = () => {
    setLoading(true);
    setError('');
    
    Promise.all([
      getBooks({ search: searchQuery, page: currentPage, size: itemsPerPage }).then(res => {
        setBooks(res.data.items || []);
        setTotalPages(res.data.totalPages || 1);
      }),
      isStaff ? loadAllBranches().then(branches => setBranches(branches)) : Promise.resolve()
    ])
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadData();
  }, [isStaff, currentPage, searchQuery]);

  const extractError = (err) => {
    const data = err.response?.data;
    if (data?.errors && typeof data.errors === 'object') {
      return Object.values(data.errors).flat().join(' ');
    }
    return data?.detail || data?.title || 'Operation failed. Please try again.';
  };

  const handleEditClick = (book) => {
    setEditingBookId(book.id);
    setForm({
      isbn: book.isbn || '',
      title: book.title || '',
      author: book.author || '',
      publisher: book.publisher || '',
      publicationYear: book.publicationYear || new Date().getFullYear(),
      category: book.category || '',
      totalCopies: book.totalCopies || 1,
      availableCopies: book.availableCopies || 0,
      branchId: book.branchId || '',
    });
    setFormError('');
    setShowForm(true);
  };

  const handleCreateClick = () => {
    setEditingBookId(null);
    setForm(emptyForm);
    setFormError('');
    setShowForm(true);
  };

  const handleCancelForm = () => {
    setShowForm(false);
    setEditingBookId(null);
    setForm(emptyForm);
  };

  const handleChange = (e) => {
    const value = e.target.type === 'number' ? Number(e.target.value) : e.target.value;
    setForm({ ...form, [e.target.name]: value });
  };

  const handleSearchChange = (e) => {
    setSearchInput(e.target.value);
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setSearchQuery(searchInput);
    setCurrentPage(1);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);

    try {
      if (editingBookId) {
        // UpdateBookCommand doesn't take BranchId, but takes AvailableCopies
        const payload = {
          id: editingBookId,
          isbn: form.isbn,
          title: form.title,
          author: form.author,
          publisher: form.publisher,
          publicationYear: form.publicationYear,
          category: form.category,
          totalCopies: form.totalCopies,
          availableCopies: form.availableCopies,
        };
        await updateBook(editingBookId, payload);
      } else {
        // CreateBookCommand requires BranchId, doesn't take AvailableCopies
        if (!form.branchId) {
          setFormError('Please select a branch.');
          setSubmitting(false);
          return;
        }
        const payload = {
          isbn: form.isbn,
          title: form.title,
          author: form.author,
          publisher: form.publisher,
          publicationYear: form.publicationYear,
          category: form.category,
          totalCopies: form.totalCopies,
          branchId: form.branchId,
        };
        await createBook(payload);
      }
      
      setShowForm(false);
      setEditingBookId(null);
      setForm(emptyForm);
      loadData();
    } catch (err) {
      setFormError(extractError(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1>Books Catalog</h1>
        {isStaff && !showForm && (
          <button className="btn btn-primary" onClick={handleCreateClick}>
            + Add New Book
          </button>
        )}
      </div>

      {showForm && (
        <section className="panel">
          <h2>{editingBookId ? 'Edit Book' : 'Add New Book'}</h2>
          <form className="form-grid" onSubmit={handleSubmit}>
            <label className="form-field">
              <span>Title</span>
              <input name="title" value={form.title} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Author</span>
              <input name="author" value={form.author} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>ISBN</span>
              <input name="isbn" value={form.isbn} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Publisher</span>
              <input name="publisher" value={form.publisher} onChange={handleChange} />
            </label>
            <label className="form-field">
              <span>Publication Year</span>
              <input type="number" name="publicationYear" value={form.publicationYear} onChange={handleChange} required />
            </label>
            <label className="form-field">
              <span>Category</span>
              <input name="category" value={form.category} onChange={handleChange} />
            </label>
            <label className="form-field">
              <span>Total Copies</span>
              <input type="number" min="1" name="totalCopies" value={form.totalCopies} onChange={handleChange} required />
            </label>
            
            {editingBookId ? (
              <label className="form-field">
                <span>Available Copies</span>
                <input type="number" min="0" max={form.totalCopies} name="availableCopies" value={form.availableCopies} onChange={handleChange} required />
              </label>
            ) : (
              <label className="form-field">
                <span>Assign to Branch</span>
                <select name="branchId" value={form.branchId} onChange={handleChange} required>
                  <option value="" disabled>-- Select a Branch --</option>
                  {branches.map(b => (
                    <option key={b.id} value={b.id}>{b.name}</option>
                  ))}
                </select>
              </label>
            )}

            <div className="form-actions">
              <button className="btn btn-outline" type="button" onClick={handleCancelForm} disabled={submitting}>
                Cancel
              </button>
              <button className="btn btn-primary" type="submit" disabled={submitting}>
                {submitting ? 'Saving...' : 'Save Book'}
              </button>
            </div>
          </form>
          {formError && <p className="error">{formError}</p>}
        </section>
      )}

      {loading && <p className="muted">Loading books...</p>}
      {error && <p className="error">Failed to load books: {error}</p>}
      
      {!loading && !error && (
        <>
          <form onSubmit={handleSearchSubmit} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
            <input
              type="text"
              placeholder="Search by title, author, or ISBN..."
              value={searchInput}
              onChange={handleSearchChange}
              style={{ padding: '0.5rem', width: '100%', maxWidth: '400px' }}
            />
            <button type="submit" className="btn btn-primary">Search</button>
          </form>
          {books.length > 0 ? (
            <>
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Author</th>
                    <th>ISBN</th>
                    <th>Available</th>
                    {isStaff && <th>Actions</th>}
                  </tr>
                </thead>
                <tbody>
                  {books.map((book) => (
                    <tr key={book.id}>
                      <td>{book.title}</td>
                      <td>{book.author}</td>
                      <td>{book.isbn}</td>
                      <td>{book.availableCopies} / {book.totalCopies}</td>
                      {isStaff && (
                        <td>
                          <button className="btn btn-outline btn-sm" onClick={() => handleEditClick(book)}>
                            Edit
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1rem' }}>
                <button
                  className="btn btn-outline"
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  Previous
                </button>
                <span>Page {currentPage} of {totalPages}</span>
                <button
                  className="btn btn-outline"
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                >
                  Next
                </button>
              </div>
            </>
          ) : (
            <p className="muted">No books found.</p>
          )}
        </>
      )}
    </div>
  );
}
