import { useEffect, useState } from 'react';
import { createMember, getMembers } from '../services/memberService';

const emptyForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  membershipExpiryDate: '',
};

export default function MemberManagement() {
  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState('');

  const loadMembers = () => {
    setLoading(true);
    setError('');
    getMembers()
      .then(({ data }) => setMembers(data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadMembers, []);

  const handleChange = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setFormError('');
    setMessage('');

    const payload = {
      ...form,
      membershipExpiryDate: new Date(form.membershipExpiryDate).toISOString(),
    };

    setSubmitting(true);
    try {
      await createMember(payload);
      setForm(emptyForm);
      setMessage('Member created successfully.');
      loadMembers();
    } catch (err) {
      setFormError(err.response?.data?.title ?? 'Failed to create member.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h1>Member Management</h1>

      <section className="panel">
        <h2>Register New Member</h2>
        <form className="form-grid" onSubmit={handleSubmit}>
          <label className="form-field">
            <span>First Name</span>
            <input name="firstName" value={form.firstName} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Last Name</span>
            <input name="lastName" value={form.lastName} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Email</span>
            <input type="email" name="email" value={form.email} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Phone</span>
            <input name="phone" value={form.phone} onChange={handleChange} required />
          </label>
          <label className="form-field">
            <span>Membership Expiry</span>
            <input type="date" name="membershipExpiryDate" value={form.membershipExpiryDate} onChange={handleChange} required />
          </label>
          <div className="form-actions">
            <button className="btn btn-primary" type="submit" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create Member'}
            </button>
          </div>
        </form>
        {formError && <p className="error">{formError}</p>}
        {message && <p className="success">{message}</p>}
      </section>

      <section className="panel">
        <h2>Members</h2>
        {loading && <p className="muted">Loading members...</p>}
        {error && <p className="error">Failed to load members: {error}</p>}
        {!loading && !error && (
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Expiry</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {members.map((member) => (
                <tr key={member.id}>
                  <td>
                    {member.firstName} {member.lastName}
                  </td>
                  <td>{member.email}</td>
                  <td>{member.phone}</td>
                  <td>{new Date(member.membershipExpiryDate).toLocaleDateString()}</td>
                  <td>{member.isActive ? 'Active' : 'Inactive'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {!loading && !error && members.length === 0 && <p className="muted">No members found.</p>}
      </section>
    </div>
  );
}
