import React, { useState } from 'react';
import { Scene } from '../types';
import { Copy, Check, Video, Image as ImageIcon, Terminal } from 'lucide-react';

interface SceneCardProps {
  scene: Scene;
}

export const SceneCard: React.FC<SceneCardProps> = ({ scene }) => {
  const [copiedImage, setCopiedImage] = useState(false);
  const [copiedVideo, setCopiedVideo] = useState(false);

  const copyToClipboard = async (text: string, isImage: boolean) => {
    try {
      await navigator.clipboard.writeText(text);
      if (isImage) {
        setCopiedImage(true);
        setTimeout(() => setCopiedImage(false), 2000);
      } else {
        setCopiedVideo(true);
        setTimeout(() => setCopiedVideo(false), 2000);
      }
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

  return (
    <div className="border border-emerald-900/50 bg-zinc-950 p-0 mb-8 shadow-lg shadow-black/50 relative overflow-hidden group">
      {/* Header */}
      <div className="bg-emerald-950/20 border-b border-emerald-900/50 p-3 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <span className="bg-emerald-900/50 text-emerald-400 px-2 py-1 text-xs font-bold font-display rounded-sm border border-emerald-700/50">
            SCENE_{scene.sceneNumber}
          </span>
          <h3 className="text-emerald-100 font-bold text-sm tracking-wide uppercase truncate">
            {scene.title}
          </h3>
        </div>
        <div className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse shadow-[0_0_8px_rgba(16,185,129,0.8)]"></div>
      </div>

      {/* Content */}
      <div className="p-5 space-y-6 relative">
        {/* Decorative Grid Background */}
        <div className="absolute inset-0 bg-[linear-gradient(rgba(16,185,129,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(16,185,129,0.03)_1px,transparent_1px)] bg-[size:20px_20px] pointer-events-none"></div>

        {/* Image Command */}
        <div className="relative z-10">
          <div className="flex items-center justify-between mb-2">
            <div className="flex items-center gap-2 text-xs font-bold text-emerald-600 uppercase">
              <ImageIcon size={14} />
              <span>Nano Banana 3 Pro</span>
            </div>
            <button 
              onClick={() => copyToClipboard(scene.imageCommand, true)}
              className="text-xs flex items-center gap-1 text-emerald-500 hover:text-emerald-300 transition-colors uppercase font-bold"
            >
              {copiedImage ? <Check size={14} /> : <Copy size={14} />}
              {copiedImage ? 'COPIED' : 'COPY CMD'}
            </button>
          </div>
          <div className="bg-black/80 border border-emerald-900/50 p-4 font-mono text-xs leading-relaxed text-emerald-100/80 break-words hover:border-emerald-500/50 transition-colors">
            {scene.imageCommand}
          </div>
        </div>

        {/* Video Command */}
        <div className="relative z-10">
          <div className="flex items-center justify-between mb-2">
            <div className="flex items-center gap-2 text-xs font-bold text-emerald-600 uppercase">
              <Video size={14} />
              <span>Motion Prompt</span>
            </div>
            <button 
              onClick={() => copyToClipboard(scene.videoCommand, false)}
              className="text-xs flex items-center gap-1 text-emerald-500 hover:text-emerald-300 transition-colors uppercase font-bold"
            >
              {copiedVideo ? <Check size={14} /> : <Copy size={14} />}
              {copiedVideo ? 'COPIED' : 'COPY CMD'}
            </button>
          </div>
          <div className="border-l-2 border-emerald-600 pl-4 py-1">
             <p className="font-mono text-xs italic text-emerald-200/70">
              {scene.videoCommand}
             </p>
          </div>
        </div>
      </div>
      
      {/* Corner decoration */}
      <div className="absolute bottom-0 right-0 w-0 h-0 border-b-[20px] border-r-[20px] border-b-emerald-900/50 border-r-transparent transform rotate-180"></div>
    </div>
  );
};
