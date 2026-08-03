import { forwardRef } from 'react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const CustomInput = forwardRef(({ value, onClick, placeholder, ...props }, ref) => (
  <input
    {...props}
    ref={ref}
    type="text"
    className="form-control"
    value={value}
    onClick={onClick}
    placeholder={placeholder || 'Select date...'}
    readOnly
    style={{ cursor: 'pointer' }}
  />
));
CustomInput.displayName = 'CustomInput';

export default function DateInput({ selected, onChange, minDate, maxDate, placeholderText }) {
  return (
    <DatePicker
      selected={selected}
      onChange={(date) => {
        onChange(date);
      }}
      minDate={minDate}
      maxDate={maxDate}
      placeholderText={placeholderText || 'Select date...'}
      customInput={<CustomInput />}
      dateFormat="MMM d, yyyy"
      className="form-control"
      calendarClassName="lms-calendar"
      showMonthDropdown
      showYearDropdown
      dropdownMode="select"
    />
  );
}
