import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Award, X } from 'lucide-react';

interface AchievementToastProps {
  title: string;
  description: string;
  show: boolean;
  onClose: () => void;
}

export const AchievementToast: React.FC<AchievementToastProps> = ({ title, description, show, onClose }) => {
  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ y: -100, opacity: 0 }}
          animate={{ y: 20, opacity: 1 }}
          exit={{ y: -100, opacity: 0 }}
          className="fixed top-0 left-1/2 -translate-x-1/2 z-[200] w-[90%] max-w-sm"
        >
          <div className="bg-gradient-to-r from-yellow-500 to-orange-600 p-[2px] rounded-2xl shadow-[0_10px_30px_rgba(245,158,11,0.3)]">
            <div className="bg-slate-900 rounded-[14px] p-4 flex items-center gap-4">
              <div className="w-12 h-12 bg-yellow-500/20 rounded-xl flex items-center justify-center">
                <Award className="w-7 h-7 text-yellow-500" />
              </div>
              <div className="flex-1">
                <p className="text-[10px] text-yellow-500 font-black uppercase tracking-widest">New Achievement!</p>
                <h4 className="text-white font-black text-lg leading-tight">{title}</h4>
                <p className="text-slate-400 text-xs">{description}</p>
              </div>
              <button onClick={onClose} className="text-slate-500 hover:text-white transition-colors">
                <X className="w-5 h-5" />
              </button>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};


