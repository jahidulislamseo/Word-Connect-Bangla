import { useState, useEffect, useMemo, useRef } from 'react';


import { motion, AnimatePresence } from 'framer-motion';
import confetti from 'canvas-confetti';
import { Trophy, Star, RefreshCw, HelpCircle, Volume2, ChevronRight, Zap, Book, Mic, Settings } from 'lucide-react';


import dictionaryData from './data/dictionary.json';
import { ParticleBackground } from './components/ParticleBackground';
import { AchievementToast } from './components/AchievementToast';
import { Leaderboard } from './components/Leaderboard';
import { VocabularyBook } from './components/VocabularyBook';
import { SettingsModal } from './components/SettingsModal';






// --- Types ---
interface WordEntry {
  bn: string;
  phonetic: string;
  audio: string;
  category: string;
  difficulty: number;
  usage?: string;
}


interface Dictionary {
  [key: string]: WordEntry;
}

const dictionary = dictionaryData as Dictionary;

// --- Constants ---
const CATEGORY_BGS: Record<string, string> = {
  nature: '/play/images/nature_bg.png',
  animal: '/play/images/animal_bg.png',
  fruit: '/play/images/fruit_bg.png',
  place: '/play/images/place_bg.png',
  abstract: '/play/images/abstract_bg.png',
  food: '/play/images/fruit_bg.png',
  object: '/play/images/abstract_bg.png',
  action: '/play/images/abstract_bg.png',
  person: '/play/images/place_bg.png',
};

const DEFAULT_BG = '/play/images/abstract_bg.png';


// --- Utils ---
const playSound = (type: 'select' | 'success' | 'error' | 'click') => {
  // Global Sound Setting Check
  const savedSettings = localStorage.getItem('gameSettings');
  const isSoundEnabled = savedSettings ? JSON.parse(savedSettings).sound : true;
  if (!isSoundEnabled) return;

  const ctx = new (window.AudioContext || (window as any).webkitAudioContext)();

  const osc = ctx.createOscillator();
  const gain = ctx.createGain();
  
  osc.connect(gain);
  gain.connect(ctx.destination);
  
  const now = ctx.currentTime;
  
  if (type === 'select') {
    osc.type = 'sine';
    osc.frequency.setValueAtTime(440 + Math.random() * 100, now);
    gain.gain.setValueAtTime(0.1, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.1);
    osc.start(now);
    osc.stop(now + 0.1);
  } else if (type === 'success') {
    // Primary Tone
    osc.type = 'triangle';
    osc.frequency.setValueAtTime(523.25, now); // C5
    osc.frequency.exponentialRampToValueAtTime(1046.50, now + 0.4); // C6
    
    // Sub Harmonic for Richness
    const osc2 = ctx.createOscillator();
    const gain2 = ctx.createGain();
    osc2.type = 'sine';
    osc2.frequency.setValueAtTime(261.63, now); // C4
    osc2.frequency.exponentialRampToValueAtTime(523.25, now + 0.4);
    osc2.connect(gain2);
    gain2.connect(ctx.destination);
    
    gain.gain.setValueAtTime(0.2, now);
    gain.gain.exponentialRampToValueAtTime(0.01, now + 0.5);
    gain2.gain.setValueAtTime(0.1, now);
    gain2.gain.exponentialRampToValueAtTime(0.01, now + 0.5);
    
    osc.start(now);
    osc2.start(now);
    osc.stop(now + 0.5);
    osc2.stop(now + 0.5);
  } else if (type === 'error') {
    osc.type = 'sawtooth';
    osc.frequency.setValueAtTime(150, now);
    osc.frequency.linearRampToValueAtTime(100, now + 0.2);
    gain.gain.setValueAtTime(0.1, now);
    gain.gain.linearRampToValueAtTime(0.01, now + 0.2);
    osc.start(now);
    osc.stop(now + 0.2);
  } else if (type === 'click') {
    osc.type = 'sine';
    osc.frequency.setValueAtTime(800, now);
    gain.gain.setValueAtTime(0.05, now);
    gain.gain.linearRampToValueAtTime(0.01, now + 0.05);
    osc.start(now);
    osc.stop(now + 0.05);
  }

};

const speak = (text: string) => {
  if ('speechSynthesis' in window) {
    window.speechSynthesis.cancel();
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'en-US';
    utterance.rate = 0.85;
    utterance.pitch = 1;
    utterance.volume = 1;
    window.speechSynthesis.speak(utterance);
  }
};

const shuffle = (array: any[]) => {
  const newArray = [...array];
  for (let i = newArray.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [newArray[i], newArray[j]] = [newArray[j], newArray[i]];
  }
  return newArray;
};

const getExample = (word: string): string => {
  const examples: Record<string, string> = {
    apple: "I eat an apple every day.",
    bird: "A bird is singing on the tree.",
    book: "I like to read this book.",
    cake: "She baked a delicious cake.",
    door: "Please close the door.",
    fish: "The fish is swimming in the water.",
    gold: "Her ring is made of gold.",
    hand: "Wash your hands before eating.",
    jump: "Can you jump over the fence?",
    king: "The king lives in a palace.",
  };
  return examples[word.toLowerCase()] || `The word "${word}" is commonly used in daily conversation.`;
};


// --- Components ---

export default function App() {
  const [level, setLevel] = useState(0);
  const [targetWord, setTargetWord] = useState('');
  const [letters, setLetters] = useState<string[]>([]);
  const [selectedIndices, setSelectedIndices] = useState<number[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [foundWords, setFoundWords] = useState<string[]>([]);
  const [bonusWords, setBonusWords] = useState<string[]>([]);
  const [showPopup, setShowPopup] = useState(false);
  const [lastFoundEntry, setLastFoundEntry] = useState<WordEntry | null>(null);
  const [isWrong, setIsWrong] = useState(false);
  const [score, setScore] = useState(100); // Start with some coins for hints
  const [combo, setCombo] = useState(0);
  const [hintsUsed, setHintsUsed] = useState<number[]>([]);
  const [showAdPopup, setShowAdPopup] = useState(false);

  const [showDailyPopup, setShowDailyPopup] = useState(false);
  const [isBossLevel, setIsBossLevel] = useState(false);
  const [achievement, setAchievement] = useState<{ title: string; description: string; show: boolean } | null>(null);
  const [showLeaderboard, setShowLeaderboard] = useState(false);
  const [collectedWords, setCollectedWords] = useState<string[]>([]);
  const [showVocabBook, setShowVocabBook] = useState(false);
  const [lastComboText, setLastComboText] = useState<string | null>(null);
  const [isListening, setIsListening] = useState(false);
  const [voiceBonusClaimed, setVoiceBonusClaimed] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [settings, setSettings] = useState({ sound: true, haptics: true });





  
  // Initialize Level
  const sortedWords = useMemo(() => {
    return Object.entries(dictionary)
      .sort(([, a], [, b]) => {
        if (a.difficulty !== b.difficulty) return a.difficulty - b.difficulty;
        return a.bn.length - b.bn.length;
      })
      .map(([word]) => word.toUpperCase());
  }, []);

  useEffect(() => {
    const word = sortedWords[level % sortedWords.length];
    const boss = (level + 1) % 5 === 0;
    setIsBossLevel(boss);
    setTargetWord(word);
    setLetters(shuffle(word.split('')));
    setFoundWords([]);
    setSelectedIndices([]);
    setIsWrong(false);

    // Achievement check
    if (level === 4) {
      setAchievement({ 
        title: "Novice Solver", 
        description: "You've reached Level 5! Great start.", 
        show: true 
      });
    } else if (level === 9) {
      setAchievement({ 
        title: "Word Enthusiast", 
        description: "10 levels completed! You're on fire.", 
        show: true 
      });
    }
  }, [level, sortedWords]);



  const currentSelectionString = useMemo(() => {
    return selectedIndices.map(i => letters[i]).join('');
  }, [selectedIndices, letters]);

  useEffect(() => {
    // Check Daily Reward
    const lastClaim = localStorage.getItem('lastDailyClaim');
    const today = new Date().toDateString();
    if (lastClaim !== today) {
      setShowDailyPopup(true);
    }


    // Load Collected Words
    const saved = localStorage.getItem('wordCollection');
    if (saved) {
      try {
        setCollectedWords(JSON.parse(saved));
      } catch (e) {
        console.error("Failed to load collection", e);
      }
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('wordCollection', JSON.stringify(collectedWords));
  }, [collectedWords]);

  // Load Settings
  useEffect(() => {
    const saved = localStorage.getItem('gameSettings');
    if (saved) setSettings(JSON.parse(saved));
  }, []);

  const saveSettings = (newSettings: typeof settings) => {
    setSettings(newSettings);
    localStorage.setItem('gameSettings', JSON.stringify(newSettings));
  };



  const handleStart = (index: number) => {
    setIsDragging(true);
    setSelectedIndices([index]);
    setIsWrong(false);
    playSound('select');
  };

  const handleEnter = (index: number) => {
    if (isDragging && !selectedIndices.includes(index)) {
      setSelectedIndices(prev => [...prev, index]);
      playSound('select');
    }
  };

  const handleEnd = () => {
    if (!isDragging) return;
    setIsDragging(false);

    const word = currentSelectionString;
    const lowerWord = word.toLowerCase();

    if (word === targetWord && !foundWords.includes(word)) {
      setFoundWords(prev => [...prev, word]);
      setLastFoundEntry(dictionary[lowerWord]);
      setShowPopup(true);
      setScore(s => s + 100 + (combo * 20));
      setCombo(c => c + 1);
      
      // Update Collection
      if (!collectedWords.includes(lowerWord)) {
        setCollectedWords(prev => [...prev, lowerWord]);
      }

      // Combo Text
      if (combo >= 2) {
        const texts = ["GOOD!", "AWESOME!", "UNSTOPPABLE!", "MASTER!", "LEGENDARY!"];
        setLastComboText(texts[Math.min(combo - 2, texts.length - 1)]);
        setTimeout(() => setLastComboText(null), 1000);
      }

      playSound('success');

      speak(word);
      confetti({
        particleCount: 150,
        spread: 100,
        origin: { y: 0.6 },
        colors: ['#22d3ee', '#8b5cf6', '#facc15']
      });
    } else if (dictionary[lowerWord] && !bonusWords.includes(word) && word !== targetWord) {
      setBonusWords(prev => [...prev, word]);
      setScore(s => s + 50);
      playSound('success');
      speak(word);
      confetti({
        particleCount: 20,
        spread: 30,
        origin: { y: 0.8 },
        colors: ['#facc15']
      });
    } else if (word.length >= 2) {
      setIsWrong(true);
      setCombo(0);
      playSound('error');
      setTimeout(() => setIsWrong(false), 500);
    }
    
    setTimeout(() => {
      setSelectedIndices([]);
    }, 150);
  };

  const nextLevel = () => {
    playSound('click');
    setShowPopup(false);
    setLevel(prev => prev + 1);
    setBonusWords([]);
    setHintsUsed([]);
    setVoiceBonusClaimed(false);
  };


  const useHint = () => {
    if (score >= 50 && hintsUsed.length < targetWord.length) {
      const availableIndices = targetWord.split('')
        .map((_, i) => i)
        .filter(i => !hintsUsed.includes(i));
      
      if (availableIndices.length > 0) {
        playSound('click');
        const randomIdx = availableIndices[Math.floor(Math.random() * availableIndices.length)];
        setHintsUsed(prev => [...prev, randomIdx]);
        setScore(s => s - 50);
      }
    } else {
      playSound('error');
    }
  };

  const claimDaily = () => {
    setScore(s => s + 200);
    localStorage.setItem('lastDailyClaim', new Date().toDateString());
    setShowDailyPopup(false);

    playSound('success');
  };

  const watchAd = () => {
    setShowAdPopup(true);
    setTimeout(() => {
      setShowAdPopup(false);
      setScore(s => s + 100);
      playSound('success');
    }, 3000); // Simulate 3s video ad
  };

  const shuffleLetters = () => {
    setLetters(shuffle(letters));
  };

  const startVoiceCheck = () => {
    if (!('webkitSpeechRecognition' in window)) {
      alert("Speech recognition is not supported in this browser.");
      return;
    }

    const recognition = new (window as any).webkitSpeechRecognition();
    recognition.lang = 'en-US';
    recognition.interimResults = false;
    recognition.maxAlternatives = 1;

    setIsListening(true);
    recognition.start();

    recognition.onresult = (event: any) => {
      const speechToText = event.results[0][0].transcript.toLowerCase();
      console.log("Speech Result:", speechToText);
      
      if (speechToText.includes(targetWord.toLowerCase())) {
        setScore(s => s + 50);
        setVoiceBonusClaimed(true);
        playSound('success');
        confetti({
          particleCount: 40,
          spread: 50,
          origin: { y: 0.6 },
          colors: ['#22d3ee']
        });
      } else {
        playSound('error');
      }
      setIsListening(false);
    };

    recognition.onerror = () => {
      setIsListening(false);
      playSound('error');
    };

    recognition.onend = () => {
      setIsListening(false);
    };
  };


  const currentCategory = lastFoundEntry?.category || dictionary[targetWord.toLowerCase()]?.category || 'abstract';
  const bgImage = CATEGORY_BGS[currentCategory] || DEFAULT_BG;

  // Calculate positions for circular tiles dynamically based on screen size
  const [boardSize, setBoardSize] = useState(300);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const updateSize = () => {
      if (containerRef.current) {
        const width = containerRef.current.offsetWidth;
        setBoardSize(Math.min(width, 300));
      }
    };
    updateSize();
    window.addEventListener('resize', updateSize);
    return () => window.removeEventListener('resize', updateSize);
  }, []);

  const radius = boardSize * 0.35;
  const tilePositions = letters.map((_, i) => {
    const angle = (i * (360 / letters.length) - 90) * (Math.PI / 180);
    const center = boardSize / 2;
    return {
      x: Math.cos(angle) * radius + center - 32,
      y: Math.sin(angle) * radius + center - 32
    };
  });

  return (
    <div className="relative flex flex-col items-center justify-between h-full p-4 md:p-6 text-white overflow-hidden transition-all duration-1000"
         onMouseUp={handleEnd}
         onTouchEnd={handleEnd}>
      
      {/* Dynamic Background */}
      <div className="absolute inset-0 z-[-1] overflow-hidden">
        <AnimatePresence mode="wait">
          <motion.img
            key={bgImage}
            src={bgImage}
            initial={{ opacity: 0, scale: 1.1 }}
            animate={{ opacity: 0.4, scale: 1 }}
            exit={{ opacity: 0, scale: 0.9 }}
            transition={{ duration: 1.5 }}
            className="w-full h-full object-cover filter blur-[2px]"
          />
        </AnimatePresence>
        <div className="absolute inset-0 bg-slate-950/40" />
        <ParticleBackground />
      </div>

      
      {/* Header */}
      <header className="w-full flex justify-between items-center max-w-md z-10">
        <div className="flex items-center gap-3">
          <div className={`p-2 glass rounded-2xl border-yellow-500/30 ${isBossLevel ? 'bg-orange-500/20 shadow-[0_0_15px_rgba(249,115,22,0.5)]' : ''}`}>
            <Trophy className={`w-6 h-6 ${isBossLevel ? 'text-orange-500 animate-pulse' : 'text-yellow-400'} drop-shadow-[0_0_8px_rgba(250,204,21,0.5)]`} />
          </div>
          <div>
            <p className={`text-[10px] ${isBossLevel ? 'text-orange-400' : 'text-slate-400'} font-black uppercase tracking-widest`}>
              {isBossLevel ? '🔥 BOSS LEVEL' : `Level ${level + 1}`}
            </p>
            <p className="text-2xl font-black leading-none">{score.toLocaleString()}</p>
          </div>

        </div>
        
        <div className="flex items-center gap-2">
          <button 
            onClick={watchAd}
            className="flex items-center gap-2 px-3 py-2 bg-gradient-to-r from-orange-500 to-red-600 rounded-xl font-black text-[10px] shadow-lg hover:scale-105 transition-all"
          >
            <Zap className="w-3 h-3 fill-white" />
            FREE COINS
          </button>
          
          <button 
            onClick={() => setShowLeaderboard(true)}
            className="p-3 glass rounded-2xl hover:bg-slate-700/50 transition-all active:scale-90"
          >
            <Trophy className="w-5 h-5 text-yellow-400" />
          </button>

          <button 
            onClick={() => setShowSettings(true)}
            className="p-3 glass rounded-2xl hover:bg-slate-700/50 transition-all active:scale-90"
          >
            <Settings className="w-5 h-5 text-slate-300" />
          </button>
        </div>



          <button 
            onClick={() => setShowVocabBook(true)}
            className="p-3 glass rounded-2xl hover:bg-slate-700/50 transition-all active:scale-90 relative"
          >
            <Book className="w-5 h-5 text-cyan-400" />
            {collectedWords.length > 0 && (
              <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 text-[8px] font-black flex items-center justify-center rounded-full animate-pulse">
                {collectedWords.length}
              </span>
            )}
          </button>

          <button className="p-3 glass rounded-2xl hover:bg-slate-700/50 transition-all active:scale-90" onClick={() => setLevel(level)}>

          <RefreshCw className="w-5 h-5 text-slate-300" />
        </button>
      </header>

      {/* Main Content */}
      <main className="flex-1 flex flex-col items-center justify-center gap-10 w-full z-10">
        {/* Word Display Slots */}
        <div className="flex flex-col items-center gap-4">
          <div className="flex gap-2">
            {targetWord.split('').map((char, i) => (
              <motion.div
                key={i}
                initial={false}
                animate={{ 
                  scale: (foundWords.includes(targetWord) || hintsUsed.includes(i)) ? [1, 1.1, 1] : 1,
                  borderColor: (foundWords.includes(targetWord) || hintsUsed.includes(i)) 
                    ? (isBossLevel ? '#f97316' : '#facc15') 
                    : 'rgba(255,255,255,0.1)',
                  backgroundColor: (foundWords.includes(targetWord) || hintsUsed.includes(i)) 
                    ? (isBossLevel ? 'rgba(249,115,22,0.1)' : 'rgba(250,204,21,0.1)') 
                    : 'rgba(30,41,59,0.7)'
                }}
                className={`w-11 h-14 glass flex items-center justify-center border-2 text-2xl font-black rounded-xl shadow-lg ${isBossLevel && !foundWords.includes(targetWord) ? 'border-orange-500/30' : ''}`}
              >
                {(foundWords.includes(targetWord) || hintsUsed.includes(i)) ? char : ''}
              </motion.div>

            ))}
          </div>
          <div className="flex items-center gap-2">
            <span className="px-3 py-1 glass rounded-lg text-[10px] font-bold text-slate-400 uppercase tracking-widest border-white/5">
              {currentCategory}
            </span>
            {bonusWords.length > 0 && (
              <motion.span 
                initial={{ scale: 0 }} 
                animate={{ scale: 1 }}
                className="px-3 py-1 bg-yellow-500/20 rounded-lg text-[10px] font-bold text-yellow-400 uppercase tracking-widest border border-yellow-500/20"
              >
                Bonus: {bonusWords.length}
              </motion.span>
            )}
          </div>
        </div>

        {/* Current Selection Preview & Combo */}
        <div className="h-10 flex flex-col items-center justify-center relative">
          <AnimatePresence>
            {lastComboText && (
              <motion.div
                initial={{ opacity: 0, scale: 0.5, y: 0 }}
                animate={{ opacity: 1, scale: 1.5, y: -40 }}
                exit={{ opacity: 0, scale: 2, y: -80 }}
                className="absolute text-2xl font-black text-yellow-400 italic tracking-tighter drop-shadow-[0_0_10px_rgba(250,204,21,0.5)]"
              >
                {lastComboText}
              </motion.div>
            )}
            
            {currentSelectionString && (

              <motion.div
                initial={{ opacity: 0, scale: 0.5, y: 20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 1.5, filter: 'blur(10px)' }}
                className={`text-4xl font-black tracking-[0.2em] ${isWrong ? 'text-red-500 shake' : 'neon-text-cyan'}`}
              >
                {currentSelectionString}
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Interaction Board */}
        <motion.div 
          ref={containerRef}
          className="relative"
          style={{ width: boardSize, height: boardSize }}
          animate={foundWords.length > 0 ? { scale: [1, 1.05, 1], rotate: [0, 1, -1, 0] } : {}}
          transition={{ duration: 0.4 }}
        >
          {/* Success Glow Ring */}
          <AnimatePresence>
            {showPopup && (
              <motion.div
                initial={{ scale: 0, opacity: 1 }}
                animate={{ scale: 2.5, opacity: 0 }}
                exit={{ opacity: 0 }}
                className="absolute inset-0 border-4 border-cyan-400 rounded-full z-0"
              />
            )}
          </AnimatePresence>

          {/* SVG Connection Lines */}
          <svg className="absolute inset-0 w-full h-full pointer-events-none overflow-visible">
            <defs>
              <filter id="glow" x="-20%" y="-20%" width="140%" height="140%">
                <feGaussianBlur stdDeviation="3" result="blur" />
                <feComposite in="SourceGraphic" in2="blur" operator="over" />
              </filter>
            </defs>
            {selectedIndices.length > 1 && selectedIndices.map((idx, i) => {
              if (i === 0) return null;
              const prevIdx = selectedIndices[i-1];
              const p1 = tilePositions[prevIdx];
              const p2 = tilePositions[idx];
              return (
                <motion.line
                  key={i}
                  initial={{ pathLength: 0 }}
                  animate={{ pathLength: 1 }}
                  x1={p1.x + 32} y1={p1.y + 32}
                  x2={p2.x + 32} y2={p2.y + 32}
                  stroke="#22d3ee"
                  strokeWidth="10"
                  strokeLinecap="round"
                  filter="url(#glow)"
                  opacity="0.8"
                />
              );
            })}
          </svg>

          {/* Letter Tiles */}
          {letters.map((letter, i) => (
            <motion.div
              key={i}
              className={`tile ${selectedIndices.includes(i) ? 'selected' : ''}`}
              style={{ left: tilePositions[i].x, top: tilePositions[i].y }}
              onMouseDown={() => handleStart(i)}
              onMouseEnter={() => handleEnter(i)}
              onTouchStart={() => handleStart(i)}
              onTouchMove={(e) => {
                const touch = e.touches[0];
                const element = document.elementFromPoint(touch.clientX, touch.clientY);
                const tileIdx = element?.getAttribute('data-index');
                if (tileIdx !== null && tileIdx !== undefined) {
                  handleEnter(parseInt(tileIdx));
                }
              }}
              data-index={i}
              whileTap={{ scale: 0.9 }}
            >
              {letter}
            </motion.div>
          ))}
        </motion.div>
      </main>


      {/* Footer Controls */}
      <footer className="w-full max-w-md pb-6 flex justify-between items-center z-10 px-4">
        <button 
          onClick={useHint}
          disabled={score < 50}
          className="flex items-center gap-2 glass px-5 py-3 rounded-2xl text-slate-300 font-black text-xs hover:bg-slate-700/50 transition-all border-white/5 active:scale-95 disabled:opacity-50"
        >
          <HelpCircle className="w-4 h-4 text-cyan-400" />
          HINT (50)
        </button>
        
        <button 
          onClick={shuffleLetters}
          className="p-3 glass rounded-2xl hover:bg-slate-700/50 transition-all active:scale-90"
        >
          <RefreshCw className="w-5 h-5 text-slate-300" />
        </button>

        <div className="flex gap-1">
          {[1, 2, 3].map(i => (
            <Star key={i} className={`w-5 h-5 ${i <= 3 ? 'text-yellow-400 fill-yellow-400' : 'text-slate-600'}`} />
          ))}
        </div>
      </footer>

      {/* Result Popup */}
      <AnimatePresence>
        {showPopup && lastFoundEntry && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-slate-950/90 backdrop-blur-md"
          >
            <motion.div
              initial={{ scale: 0.7, y: 50, rotate: -5 }}
              animate={{ scale: 1, y: 0, rotate: 0 }}
              className="glass max-w-sm w-full p-8 flex flex-col items-center gap-6 border-2 border-cyan-500/30 shadow-[0_0_50px_rgba(34,211,238,0.2)]"
            >
              <motion.div 
                animate={{ rotate: 360 }}
                transition={{ duration: 20, repeat: Infinity, ease: "linear" }}
                className="w-24 h-24 bg-gradient-to-br from-cyan-500 to-purple-600 rounded-full flex items-center justify-center relative"
              >
                <div className="absolute inset-1 bg-slate-900 rounded-full flex items-center justify-center">
                  <Star className="w-12 h-12 text-yellow-400 fill-yellow-400" />
                </div>
              </motion.div>
              
              <div className="text-center w-full">
                <div className="flex items-center justify-center gap-3 mb-2">
                  <h2 className="text-5xl font-black text-white uppercase tracking-tighter">
                    {targetWord}
                  </h2>
                  <button 
                    onClick={() => speak(targetWord)}
                    className="p-3 bg-cyan-500/20 rounded-2xl hover:bg-cyan-500/40 transition-all active:scale-90"
                  >
                    <Volume2 className="w-7 h-7 text-cyan-400" />
                  </button>
                </div>
                <p className="text-slate-400 font-bold italic mb-6 tracking-wide">{lastFoundEntry.phonetic}</p>
                
                <motion.div 
                  initial={{ x: -20, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  transition={{ delay: 0.3 }}
                  className="py-6 px-8 bg-gradient-to-br from-cyan-500/20 to-purple-500/10 rounded-3xl border border-cyan-500/30 w-full shadow-inner relative overflow-hidden"
                >
                  <p className="text-6xl font-black text-cyan-300 mb-2 font-bengali drop-shadow-lg">
                    {lastFoundEntry.bn}
                  </p>
                  <div className="flex items-center justify-center gap-2 mb-4">
                    <span className="px-3 py-1 bg-cyan-500/20 rounded-full text-[10px] uppercase tracking-widest text-cyan-400 font-black">
                      {lastFoundEntry.category}
                    </span>
                  </div>
                  
                  <div className="text-left bg-slate-950/50 p-4 rounded-2xl border border-white/5 relative z-10">
                    <p className="text-[10px] text-slate-500 font-black uppercase mb-1 tracking-widest">Example Usage</p>
                    <p className="text-sm text-slate-200 italic leading-relaxed">
                      "{getExample(targetWord)}"
                    </p>
                  </div>

                  {/* Voice Bonus UI */}
                  {!voiceBonusClaimed && (
                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      onClick={startVoiceCheck}
                      disabled={isListening}
                      className={`mt-4 w-full py-3 rounded-2xl border-2 border-dashed flex items-center justify-center gap-3 transition-all ${
                        isListening 
                          ? 'border-cyan-500 bg-cyan-500/20 animate-pulse' 
                          : 'border-white/10 hover:border-cyan-500/30 hover:bg-white/5'
                      }`}
                    >
                      {isListening ? (
                        <>
                          <div className="flex gap-1">
                            {[1,2,3].map(i => (
                              <motion.div 
                                key={i}
                                animate={{ height: [8, 16, 8] }}
                                transition={{ duration: 0.5, repeat: Infinity, delay: i * 0.1 }}
                                className="w-1 bg-cyan-400 rounded-full"
                              />
                            ))}
                          </div>
                          <span className="text-xs font-black text-cyan-400">LISTENING...</span>
                        </>
                      ) : (
                        <>
                          <Mic className="w-4 h-4 text-cyan-400" />
                          <span className="text-xs font-black text-slate-300 uppercase">Say it to earn +50 Bonus!</span>
                        </>
                      )}
                    </motion.button>
                  )}

                  {voiceBonusClaimed && (
                    <motion.div 
                      initial={{ scale: 0 }} 
                      animate={{ scale: 1 }}
                      className="mt-4 flex items-center justify-center gap-2 text-cyan-400 font-black text-xs uppercase"
                    >
                      <Star className="w-4 h-4 fill-cyan-400" />
                      Voice Bonus Claimed!
                    </motion.div>
                  )}
                </motion.div>


              </div>

              <button 
                onClick={nextLevel}
                className="group w-full py-5 bg-gradient-to-r from-cyan-500 to-blue-600 rounded-2xl font-black text-xl shadow-[0_10px_20px_rgba(34,211,238,0.3)] hover:shadow-[0_15px_30px_rgba(34,211,238,0.4)] hover:-translate-y-1 active:translate-y-0 transition-all flex items-center justify-center gap-3"
              >
                CONTINUE
                <ChevronRight className="w-7 h-7 group-hover:translate-x-1 transition-transform" />
              </button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Ad Simulation Popup */}
      <AnimatePresence>
        {showAdPopup && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-[100] bg-black flex flex-col items-center justify-center p-10"
          >
            <div className="w-20 h-20 border-4 border-cyan-500 border-t-transparent rounded-full animate-spin mb-6" />
            <h3 className="text-2xl font-black mb-2">WATCHING AD...</h3>
            <p className="text-slate-400">Reward arriving in 3 seconds</p>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Daily Reward Popup */}
      <AnimatePresence>
        {showDailyPopup && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-slate-950/90 backdrop-blur-md"
          >
            <motion.div
              initial={{ scale: 0.8 }}
              animate={{ scale: 1 }}
              className="glass max-w-sm w-full p-8 flex flex-col items-center gap-6 border-2 border-yellow-500/30"
            >
              <div className="w-20 h-20 bg-yellow-500/20 rounded-full flex items-center justify-center">
                <Trophy className="w-10 h-10 text-yellow-400" />
              </div>
              <div className="text-center">
                <h3 className="text-2xl font-black mb-1">DAILY REWARD!</h3>
                <p className="text-slate-400">Welcome back! Claim your daily bonus coins.</p>
              </div>
              <button 
                onClick={claimDaily}
                className="w-full py-4 bg-yellow-500 text-slate-900 font-black rounded-2xl shadow-lg hover:bg-yellow-400 transition-all"
              >
                CLAIM 200 COINS
              </button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      <AchievementToast 
        title={achievement?.title || ''} 
        description={achievement?.description || ''} 
        show={!!achievement?.show} 
        onClose={() => setAchievement(prev => prev ? { ...prev, show: false } : null)} 
      />

      <Leaderboard 
        show={showLeaderboard} 
        onClose={() => setShowLeaderboard(false)} 
        userScore={score}
      />

      <VocabularyBook 
        show={showVocabBook}
        onClose={() => setShowVocabBook(false)}
        collectedWords={collectedWords}
        dictionary={dictionary}
        onSpeak={speak}
        totalWords={Object.keys(dictionary).length}
      />

      <SettingsModal 
        show={showSettings}
        onClose={() => setShowSettings(false)}
        settings={settings}
        onUpdateSettings={saveSettings}
      />
    </div>




  );
}
