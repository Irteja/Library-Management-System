import { useEffect, useState } from 'react';
import { createBranch, getBranches } from '../services/branchService';

const emptyForm = {
  name: '',
  address: '',
  phone: '',
  email: '',
};

export default function Branches() {
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState('');

  const loadBranches = () => {
    setLoading(true);
    setError('');
    getBranches()
      .then(({ data }) => setBranches(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadBranches, []);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setFormError('');
    setMessage('');
    setSubmitting(true);
    try {
      await createBranch(form);
      setForm(emptyForm);
      setMessage('Branch created successfully.');
      loadBranches();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to create branch.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h1>Branches</h1>

      <section className="panel">
        <h2>Add New Branch</h2>
        <form className="form-grid" onSubmit={handleSubmit}>
          <label className="form-field">
            <span>Name</span>
            <input name="name" value={form.name} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Address</span>
            <input name="address" value={form.address} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Phone</span>
            <input name="phone" value={form.phone} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Email</span>
            <input type="email" name="email" value={form.email} onChange={handleChange} required />
          </label>
          <div className="form-actions">
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create Branch'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {message && <p className="success">{message}</p>}
      </section>

      <section className="panel">
        <h2>Branches</h2>
        {loading && <p className="muted">Loading branches...</p>}
        {error && <p className="error">Failed to load branches: {error}</p>}
        {!loading && !error && (
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Address</th>
                <th>Phone</th>
                <th>Email</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {branches.map((branch) => (
                <tr key={branch.id}>
                  <td>{branch.name}</td>
                  <td>{branch.address}</td>
                  <td>{branch.phone}</td>
                  <td>{branch.email}</td>
                  <td>{branch.isActive ? 'Active' : 'Inactive'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && !error && branches.length === 0 && <p className="muted">No branches found.</p>}
      </section>
    </div>
  );
}
