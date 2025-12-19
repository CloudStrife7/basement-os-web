import React from 'react';

interface TerminalInputProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  multiline?: boolean;
  rows?: number;
  placeholder?: string;
}

export const TerminalInput: React.FC<TerminalInputProps> = ({ 
  label, 
  value, 
  onChange, 
  multiline = false, 
  rows = 3,
  placeholder
}) => {
  return (
    <div className="group mb-6">
      <label className="block text-emerald-500 text-xs font-bold mb-2 uppercase tracking-widest font-display">
        <span className="mr-2 text-emerald-800 group-hover:text-emerald-400 transition-colors">[{label}]</span>
      </label>
      <div className="relative">
        <div className="absolute left-0 top-0 bottom-0 w-1 bg-emerald-900 group-hover:bg-emerald-500 transition-colors duration-300"></div>
        {multiline ? (
          <textarea
            className="w-full bg-zinc-950/50 text-emerald-100 p-3 pl-4 border-b border-r border-t border-emerald-900/30 focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500/20 text-sm font-mono placeholder-emerald-900/50 resize-y"
            rows={rows}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
          />
        ) : (
          <input
            type="text"
            className="w-full bg-zinc-950/50 text-emerald-100 p-3 pl-4 border-b border-r border-t border-emerald-900/30 focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500/20 text-sm font-mono placeholder-emerald-900/50"
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
          />
        )}
      </div>
    </div>
  );
};
