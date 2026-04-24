import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Trophy, User } from 'lucide-react';

interface LeaderboardProps {
  show: boolean;
  onClose: () => void;
  userScore: number;
}

const MOCK_LEADERS = [
  { name: "Ariful Islam", score: 15400, rank: 1 },
  { name: "Nusrat Jahan", score: 14200, rank: 2 },
  { name: "Tanvir Ahmed", score: 12800, rank: 3 },
  { name: "Sultana Razia", score: 11500, rank: 4 },
  { name: "Kamrul Hasan", score: 10200, rank: 5 },
];

export const Leaderboard: React.FC<LeaderboardProps> = ({ show, onClose, userScore }) => {
  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[150] flex items-center justify-center p-6 bg-slate-950/90 backdrop-blur-md"
        >
          <motion.div
            initial={{ scale: 0.9, y: 20 }}
            animate={{ scale: 1, y: 0 }}
            className="glass max-w-sm w-full p-6 flex flex-col gap-6 border-2 border-cyan-500/30"
          >
            <div className="flex justify-between items-center">
              <div className="flex items-center gap-3">
                <Trophy className="w-6 h-6 text-yellow-400" />
                <h3 className="text-xl font-black">Global Rankings</h3>
              </div>
              <button onClick={onClose} className="p-2 hover:bg-slate-800 rounded-lg transition-colors">
                <X className="w-6 h-6" />
              </button>
            </div>

            <div className="flex flex-col gap-3">
              {MOCK_LEADERS.map((leader) => (
                <div key={leader.rank} className="flex items-center justify-between p-3 glass rounded-xl border-white/5">
                  <div className="flex items-center gap-4">
                    <span className={`w-6 text-center font-black ${leader.rank <= 3 ? 'text-yellow-400' : 'text-slate-500'}`}>
                      {leader.rank}
                    </span>
                    <div className="w-8 h-8 bg-slate-800 rounded-lg flex items-center justify-center">
                      <User className="w-5 h-5 text-slate-400" />
                    </div>
                    <span className="font-bold text-sm">{leader.name}</span>
                  </div>
                  <span className="font-black text-cyan-400">{leader.score.toLocaleString()}</span>
                </div>
              ))}

              <div className="mt-4 p-4 bg-cyan-500/10 border-2 border-cyan-500/30 rounded-2xl flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <span className="w-6 text-center font-black text-cyan-400">99+</span>
                  <div className="w-8 h-8 bg-cyan-500/20 rounded-lg flex items-center justify-center">
                    <User className="w-5 h-5 text-cyan-400" />
                  </div>
                  <span className="font-black text-sm">YOU</span>
                </div>
                <span className="font-black text-cyan-400">{userScore.toLocaleString()}</span>
              </div>
            </div>

            <button 
              onClick={onClose}
              className="w-full py-4 bg-gradient-to-r from-cyan-500 to-blue-600 rounded-2xl font-black shadow-lg hover:shadow-cyan-500/20 transition-all active:scale-95"
            >
              KEEP PLAYING
            </button>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
