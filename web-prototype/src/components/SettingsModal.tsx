import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Volume2, VolumeX, Smartphone, RefreshCw } from 'lucide-react';

interface SettingsModalProps {
  show: boolean;
  onClose: () => void;
  settings: {
    sound: boolean;
    haptics: boolean;
  };
  onUpdateSettings: (settings: { sound: boolean; haptics: boolean }) => void;
}

export const SettingsModal: React.FC<SettingsModalProps> = ({ 
  show, 
  onClose, 
  settings, 
  onUpdateSettings 
}) => {
  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[250] flex items-center justify-center p-6 bg-slate-950/90 backdrop-blur-md"
        >
          <motion.div
            initial={{ scale: 0.9, y: 20 }}
            animate={{ scale: 1, y: 0 }}
            className="glass max-w-sm w-full p-8 flex flex-col gap-8 border-2 border-white/10 shadow-2xl"
          >
            <div className="flex justify-between items-center">
              <h3 className="text-2xl font-black tracking-widest uppercase">Settings</h3>
              <button onClick={onClose} className="p-2 hover:bg-white/5 rounded-xl transition-colors">
                <X className="w-6 h-6" />
              </button>
            </div>

            <div className="flex flex-col gap-6">
              {/* Sound Toggle */}
              <div className="flex items-center justify-between p-4 bg-white/5 rounded-2xl border border-white/5">
                <div className="flex items-center gap-4">
                  <div className={`p-3 rounded-xl ${settings.sound ? 'bg-cyan-500/20' : 'bg-slate-800'}`}>
                    {settings.sound ? <Volume2 className="w-5 h-5 text-cyan-400" /> : <VolumeX className="w-5 h-5 text-slate-500" />}
                  </div>
                  <div>
                    <p className="font-black text-sm uppercase">Sound Effects</p>
                    <p className="text-[10px] text-slate-500 font-bold uppercase">SFX & Music</p>
                  </div>
                </div>
                <button 
                  onClick={() => onUpdateSettings({ ...settings, sound: !settings.sound })}
                  className={`w-12 h-6 rounded-full p-1 transition-colors ${settings.sound ? 'bg-cyan-500' : 'bg-slate-700'}`}
                >
                  <motion.div 
                    animate={{ x: settings.sound ? 24 : 0 }}
                    className="w-4 h-4 bg-white rounded-full shadow-lg"
                  />
                </button>
              </div>

              {/* Haptics Toggle */}
              <div className="flex items-center justify-between p-4 bg-white/5 rounded-2xl border border-white/5">
                <div className="flex items-center gap-4">
                  <div className={`p-3 rounded-xl ${settings.haptics ? 'bg-purple-500/20' : 'bg-slate-800'}`}>
                    <Smartphone className={`w-5 h-5 ${settings.haptics ? 'text-purple-400' : 'text-slate-500'}`} />
                  </div>
                  <div>
                    <p className="font-black text-sm uppercase">Haptic Feedback</p>
                    <p className="text-[10px] text-slate-500 font-bold uppercase">Device Vibration</p>
                  </div>
                </div>
                <button 
                  onClick={() => onUpdateSettings({ ...settings, haptics: !settings.haptics })}
                  className={`w-12 h-6 rounded-full p-1 transition-colors ${settings.haptics ? 'bg-purple-500' : 'bg-slate-700'}`}
                >
                  <motion.div 
                    animate={{ x: settings.haptics ? 24 : 0 }}
                    className="w-4 h-4 bg-white rounded-full shadow-lg"
                  />
                </button>
              </div>

              {/* Reset Progress */}
              <button 
                onClick={() => {
                  if (confirm("Reset all progress? This cannot be undone.")) {
                    localStorage.clear();
                    window.location.reload();
                  }
                }}
                className="mt-4 flex items-center justify-center gap-3 py-4 bg-red-500/10 border border-red-500/20 rounded-2xl text-red-500 font-black text-xs uppercase hover:bg-red-500/20 transition-all"
              >
                <RefreshCw className="w-4 h-4" />
                Reset Game Data
              </button>
            </div>

            <button 
              onClick={onClose}
              className="w-full py-4 bg-slate-100 text-slate-900 rounded-2xl font-black uppercase text-sm shadow-xl active:scale-95 transition-all"
            >
              Close
            </button>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
