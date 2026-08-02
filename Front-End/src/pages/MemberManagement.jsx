import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { createMember, getMembers } from '../services/memberService';
import { getBranches } from '../services/branchService';
import api from '../services/api';

const emptyMemberForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  membershipExpiryDate: '',
  username: '',
  password: '',
};

const emptyLibrarianForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  username: '',
  password: '',
  branchId: '',
};

export default function MemberManagement() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [members, setMembers] = useState([]);
  const [branches, setBranches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Member creation state
  const [memberForm, setMemberForm] = useState(emptyMemberForm);
  const [memberFormError, setMemberFormError] = useState('');
  const [memberSubmitting, setMemberSubmitting] = useState(false);
  const loadData = () => {
    setLoading(true);
    setError('');
    
    getMembers()
      .then(res => setMembers(res.data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadData, []);

  const extractError = (err) => {
    const data = err.response?.data;
    if (data?.errors && typeof data.errors === 'object') {
      return Object.values(data.errors).flat().join(' ');
    }
    return data?.detail || data?.title || 'Operation failed. Please try again.';
  };

  // Member form handlers
  const handleMemberChange = (event) => {
    setMemberForm({ ...memberForm, [event.target.name]: event.target.value });
  };

  const handleMemberSubmit = async (event) => {
    event.preventDefault();
    setMemberFormError('');
    setMemberMessage('');

    const payload = {
      ...memberForm,
      membershipExpiryDate: new Date(memberForm.membershipExpiryDate).toISOString(),
    };

    setMemberSubmitting(true);
    try {
      await createMember(payload);
      setMemberForm(emptyMemberForm);
      setMemberMessage('Member created successfully.');
      loadData();
    } catch (err) {
      setMemberFormError(extractError(err));
    } finally {
      setMemberSubmitting(false);
    }
  };

  return (
    <div>
      <h1>Member Management</h1>

      {/* Create Member Section */}
      <section className="panel">

      {/* Members List */}
      <section className="panel">
        <h2>Members Directory</h2>
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
