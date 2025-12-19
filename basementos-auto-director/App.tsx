import React, { useState } from 'react';
import { TerminalInput } from './components/TerminalInput';
import { SceneCard } from './components/SceneCard';
import { generateManifest } from './services/geminiService';
import { DirectorConfig, Scene } from './types';
import { DEFAULT_IDENTITY, DEFAULT_SCRIPT, DEFAULT_STYLE, DEFAULT_VISUALS } from './constants';
import { Terminal, Film, Cpu, Zap, Activity } from 'lucide-react';

const App: React.FC = () => {
  const [config, setConfig] = useState<DirectorConfig>({
    characterIdentity: DEFAULT_IDENTITY,
    characterVisuals: DEFAULT_VISUALS,
    artStyle: DEFAULT_STYLE,
    script: DEFAULT_SCRIPT,
  });

  const [scenes, setScenes] = useState<Scene[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleGenerate = async () => {
    setIsLoading(true);
    setError(null);
    setScenes([]);

    try {
      const response = await generateManifest(config);
      setScenes(response.scenes);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred');
    } finally {
      setIsLoading(false);
    }
  };

  const updateConfig = (key: keyof DirectorConfig, value: string) => {
    setConfig(prev => ({ ...prev, [key]: value }));
  };

  return (
    <div className="min-h-screen bg-black text-emerald-50 font-mono selection:bg-emerald-500/30 selection:text-emerald-200">
      
      {/* Navbar / Header */}
      <header className="border-b border-emerald-900/50 bg-black/95 backdrop-blur-sm sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-3">
             <div className="bg-emerald-500/10 p-2 rounded border border-emerald-500/20">
               <Terminal size={20} className="text-emerald-500" />
             </div>
             <div>
               <h1 className="font-display font-bold text-xl tracking-widest text-emerald-100 uppercase glitch-text cursor-default">
                 BasementOS <span className="text-emerald-600 text-sm">v3.0</span>
               </h1>
               <p className="text-[10px] text-emerald-600 uppercase tracking-[0.2em]">Auto-Director Module</p>
             </div>
          </div>
          <div className="hidden md:flex items-center gap-6 text-xs text-emerald-700 font-bold uppercase tracking-widest">
            <div className="flex items-center gap-2">
              <Cpu size={14} />
              <span>System: Online</span>
            </div>
            <div className="flex items-center gap-2">
              <Activity size={14} />
              <span>Gemini: Active</span>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto p-6 grid grid-cols-1 lg:grid-cols-12 gap-8">
        
        {/* Left Column: Configuration */}
        <div className="lg:col-span-5 space-y-8">
          <div className="bg-zinc-950/50 border border-emerald-900/30 p-6 relative">
            <div className="absolute -top-3 -left-3 bg-black border border-emerald-500/50 p-1">
              <Cpu size={16} className="text-emerald-500" />
            </div>
            
            <h2 className="text-emerald-400 font-display font-bold text-lg mb-6 uppercase border-b border-emerald-900/50 pb-2 flex items-center justify-between">
              <span>Input_Parameters</span>
              <span className="text-[10px] text-emerald-700 tracking-wider">config.sys</span>
            </h2>

            <TerminalInput
              label="CHARACTER_IDENTITY"
              value={config.characterIdentity}
              onChange={(val) => updateConfig('characterIdentity', val)}
              placeholder="e.g. Neo, The Drifter..."
            />
            
            <TerminalInput
              label="CHARACTER_VISUALS"
              value={config.characterVisuals}
              onChange={(val) => updateConfig('characterVisuals', val)}
              multiline
              rows={4}
              placeholder="Describe appearance..."
            />

            <TerminalInput
              label="ART_STYLE"
              value={config.artStyle}
              onChange={(val) => updateConfig('artStyle', val)}
              multiline
              rows={3}
              placeholder="Describe aesthetic..."
            />

            <TerminalInput
              label="VOICEOVER_SCRIPT"
              value={config.script}
              onChange={(val) => updateConfig('script', val)}
              multiline
              rows={10}
              placeholder="Paste script here..."
            />

            <button
              onClick={handleGenerate}
              disabled={isLoading}
              className={`
                w-full relative group overflow-hidden px-8 py-4 
                ${isLoading ? 'bg-emerald-900/20 cursor-not-allowed' : 'bg-emerald-900/20 hover:bg-emerald-800/30'}
                border border-emerald-500/50 transition-all duration-300
              `}
            >
              <div className={`absolute inset-0 w-full h-full bg-emerald-500/10 transform -translate-x-full ${!isLoading && 'group-hover:translate-x-0'} transition-transform duration-300`}></div>
              <div className="relative flex items-center justify-center gap-3">
                {isLoading ? (
                  <>
                    <div className="h-4 w-4 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin"></div>
                    <span className="font-display font-bold text-emerald-400 tracking-widest uppercase text-sm">Processing...</span>
                  </>
                ) : (
                  <>
                    <Zap size={18} className="text-emerald-400" />
                    <span className="font-display font-bold text-emerald-400 tracking-widest uppercase text-sm">Execute_Manifest</span>
                  </>
                )}
              </div>
            </button>
          </div>
        </div>

        {/* Right Column: Output */}
        <div className="lg:col-span-7">
          <div className="h-full flex flex-col">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-emerald-400 font-display font-bold text-lg uppercase flex items-center gap-2">
                <Film size={20} />
                <span>Production_Queue</span>
              </h2>
              {scenes.length > 0 && (
                <span className="text-xs font-mono text-emerald-600 border border-emerald-900/50 px-2 py-1 bg-black">
                  {scenes.length} BLOCKS GENERATED
                </span>
              )}
            </div>

            <div className="flex-1 min-h-[500px] relative">
              {/* Scanline overlay effect */}
              <div className="absolute inset-0 pointer-events-none z-50 bg-[linear-gradient(rgba(18,18,18,0)_50%,rgba(0,0,0,0.1)_50%),linear-gradient(90deg,rgba(255,0,0,0.01),rgba(0,255,0,0.01),rgba(0,0,255,0.01))] bg-[length:100%_4px,3px_100%]"></div>
              
              {error && (
                <div className="border border-red-900/50 bg-red-950/10 p-6 text-red-400 font-mono text-sm">
                  <h3 className="font-bold uppercase mb-2 flex items-center gap-2">
                    <div className="h-2 w-2 bg-red-500 rounded-full animate-ping"></div>
                    System Error
                  </h3>
                  <p>{error}</p>
                </div>
              )}

              {!isLoading && !error && scenes.length === 0 && (
                <div className="h-full flex flex-col items-center justify-center text-emerald-900 border-2 border-dashed border-emerald-900/30 rounded-sm p-12">
                   <div className="mb-4 opacity-50">
                     <Terminal size={48} />
                   </div>
                   <p className="font-display uppercase tracking-widest text-sm text-center">Awaiting Input Stream</p>
                   <p className="font-mono text-xs text-center mt-2 max-w-xs opacity-60">
                     Enter parameters and execute manifest to begin asset generation sequence.
                   </p>
                </div>
              )}

              <div className="space-y-6 pb-12">
                {scenes.map((scene, index) => (
                  <div key={index} style={{ animationDelay: `${index * 150}ms` }} className="animate-in fade-in slide-in-from-bottom-4 duration-700 fill-mode-backwards">
                    <SceneCard scene={scene} />
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </main>
      
      {/* Footer */}
      <footer className="border-t border-emerald-900/30 bg-black py-6 text-center">
        <p className="text-emerald-900 text-[10px] uppercase tracking-[0.3em] font-display">
          Built with React + Gemini 2.5 Flash // System_Root_Access
        </p>
      </footer>
    </div>
  );
};

export default App;
