import { Link } from 'react-router-dom';

export default function Unauthorized() {
  return (
    <div className="not-found">
      <h1>403</h1>
      <p>You do not have permission to view this page.</p>
      <Link to="/" className="btn btn-primary">
        Go to Home
      </Link>
    </div>
  );
}
